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
