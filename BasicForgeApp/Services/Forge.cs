using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Newtonsoft.Json;

namespace BasicForgeApp.Services
{
    public class Auth
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }

    public class Model
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("urn")]
        public string URN { get; set; }
    }

    public interface IForgeService
    {
        Task<Auth> GetPublicToken();
        Task<List<Model>> ListModels();
    }

    public class Forge : IForgeService
    {
        private static Scope[] PUBLIC_TOKEN_SCOPES = new Scope[] { Scope.ViewablesRead };
        private static Scope[] INTERNAL_TOKEN_SCOPES = new Scope[] { Scope.BucketRead, Scope.BucketCreate, Scope.DataRead, Scope.DataWrite, Scope.DataCreate };

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string bucketKey;

        public Forge(string _clientId, string _clientSecret, string _bucketKey)
        {
            clientId = _clientId;
            clientSecret = _clientSecret;
            bucketKey = _bucketKey;
        }

        private static string Base64Encode(string str)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return System.Convert.ToBase64String(bytes).TrimEnd('=');
        }

        private async Task<Auth> GetAccessToken(Scope[] scopes)
        {
            var client = new TwoLeggedApi();
            dynamic auth = await client.AuthenticateAsync(clientId, clientSecret, "client_credentials", scopes);
            return new Auth { AccessToken = auth.access_token, ExpiresIn = auth.expires_in };
        }

        private async Task EnsureBucketExists(string bucketKey)
        {
            var auth = await GetAccessToken(INTERNAL_TOKEN_SCOPES);
            var client = new BucketsApi();
            client.Configuration.AccessToken = auth.AccessToken;
            var buckets = await client.GetBucketsAsync();
            foreach (KeyValuePair<string, dynamic> bucket in new DynamicDictionaryItems(buckets.items))
            {
                if (bucket.Value.bucketKey == bucketKey)
                {
                    return;
                }
            }
            await client.CreateBucketAsync(new PostBucketsPayload { BucketKey = bucketKey, PolicyKey = PostBucketsPayload.PolicyKeyEnum.Temporary });
        }

        public async Task<Auth> GetPublicToken()
        {
            var auth = await GetAccessToken(PUBLIC_TOKEN_SCOPES);
            return auth;
        }

        public async Task<List<Model>> ListModels()
        {
            await EnsureBucketExists(bucketKey);
            var auth = await GetAccessToken(INTERNAL_TOKEN_SCOPES);
            var client = new ObjectsApi();
            client.Configuration.AccessToken = auth.AccessToken;
            var objects = await client.GetObjectsAsync(bucketKey, 100);
            var models = new List<Model> { };
            foreach (KeyValuePair<string, dynamic> obj in new DynamicDictionaryItems(objects.items))
            {
                models.Add(new Model { Name = obj.Value.objectKey, URN = Base64Encode(obj.Value.objectId) });
            }
            return models;
        }
    }
}
