﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.Security.Authentication.Web;

namespace Xamarin.Essentials
{
    public static partial class WebAuthenticator
    {
        static async Task<AuthResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
        {
            if (!IsUriProtocolDeclared(callbackUrl.Scheme))
                throw new InvalidOperationException($"You need to declare the windows.protocol usage of the protocol/scheme `{callbackUrl.Scheme}` in your AppxManifest.xml file");

            var r = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, url, callbackUrl);

            switch (r.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    // For GET requests this is a URI:
                    var resultUri = new Uri(r.ResponseData.ToString());
                    return new AuthResult(resultUri);
                case WebAuthenticationStatus.ErrorHttp:
                    throw new UnauthorizedAccessException();
                default:
                    throw new Exception(r.ResponseData.ToString());
            }
        }

        static bool IsUriProtocolDeclared(string scheme)
        {
            var doc = XDocument.Load(Platform.AppManifestFilename, LoadOptions.None);
            var reader = doc.CreateReader();
            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
            namespaceManager.AddNamespace("x", Platform.AppManifestXmlns);
            namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

            // Check if the protocol was declared
            var decl = doc.Root.XPathSelectElements($"//uap:Extension[@Category='windows.protocol']/uap:Protocol[@Name='{scheme}']", namespaceManager);

            return decl != null && decl.Any();
        }
    }
}
