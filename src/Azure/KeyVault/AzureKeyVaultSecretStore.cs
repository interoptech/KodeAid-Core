﻿// Copyright © Kris Penner. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using KodeAid.Security.Cryptography.X509Certificates;
using KodeAid.Security.Secrets;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace KodeAid.Azure.KeyVault
{
    public class AzureKeyVaultSecretStore : ISecretReadOnlyStore, IPrivateCertificateStore
    {
        private readonly string _keyVaultBaseUrl;

        public AzureKeyVaultSecretStore(AzureKeyVaultSecretStoreOptions options)
        {
            ArgCheck.NotNull(nameof(options), options);
            options.Verify();

            _keyVaultBaseUrl = options.KeyVaultBaseUrl?.TrimEnd(' ', '/');
        }

        public async Task<SecureString> GetSecretAsync(string name, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNullOrEmpty(nameof(name), name);

            var tokenProvider = new AzureServiceTokenProvider();

            using (var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback)))
            {
                var secretBundle = await client.GetSecretAsync(_keyVaultBaseUrl, name).ConfigureAwait(false);

                var secureString = new SecureString();
                foreach (var c in secretBundle.Value)
                {
                    secureString.AppendChar(c);
                }
                secureString.MakeReadOnly();

                return secureString;
            }
        }

        async Task<X509Certificate2> IPrivateCertificateStore.GetPrivateCertificateAsync(string name, CancellationToken cancellationToken)
        {
            var securedBase64Key = await GetSecretAsync(name, cancellationToken).ConfigureAwait(false);
            return new X509Certificate2(Convert.FromBase64String(securedBase64Key.Unsecure()));
        }
    }
}