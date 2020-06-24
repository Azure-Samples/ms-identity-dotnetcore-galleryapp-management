/*
 The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace daemon_console
{
    public class SelfSignedCertificate
    {
        public byte[] PrivateKey { get;}
        public byte[] PublicKey { get; }
        public DateTime StartDateTime { get;}
        public DateTime EndDateTime { get; }
        public string Thumbprint { get; }
        public byte[] CustomKeyIdentifier { get;}
        private X509Certificate2 selfSignedCertificate;
        //Password used for opening the private key
        private string password;

        /// <summary>
        /// Constructor that creates a self-signed certificate and initialize the properties necessary
        /// to patch the keyCredential property.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="certificateName"></param>
        public SelfSignedCertificate(string password, string certificateName)
        {
            this.password = password;
            selfSignedCertificate = buildSelfSignedServerCertificate(password, certificateName);
            PrivateKey = selfSignedCertificate.Export(X509ContentType.Pfx, password);
            PublicKey = selfSignedCertificate.Export(X509ContentType.Cert);
            StartDateTime = selfSignedCertificate.NotBefore;
            EndDateTime = selfSignedCertificate.NotAfter;
            Thumbprint = selfSignedCertificate.Thumbprint;
            CustomKeyIdentifier = getCustomKeyIdentifier(Thumbprint);
        }
        /// <summary>
        /// Creates a self-signed certificate
        /// </summary>
        /// <param name="password"></param>
        /// <param name="certificateName"></param>
        /// <returns>The self-signed certificate</returns>
        private X509Certificate2 buildSelfSignedServerCertificate(string password, string certificateName)
        {
            
            DateTime certificateStartDate = DateTime.UtcNow;
            DateTime certificateEndDate = certificateStartDate.AddYears(2).ToUniversalTime();

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={certificateName}");

            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                var certificate = request.CreateSelfSigned(new DateTimeOffset(certificateStartDate), new DateTimeOffset(certificateEndDate));
                certificate.FriendlyName = certificateName;

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.Exportable);
            }
        }
        /// <summary>
        /// Generates a custom key identifier using the a certificate thumbprint
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <returns>A byte array with the Sha256 hast of the thumbprint</returns>
        private byte[] getCustomKeyIdentifier(string thumbprint)
        {
            var message = Encoding.ASCII.GetBytes(thumbprint);
            SHA256Managed hashString = new SHA256Managed();
            return hashString.ComputeHash(message);
        }


    }
}
