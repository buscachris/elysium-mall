using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MallDemo
{
    public class CatalogService : MonoBehaviour
    {
        public IEnumerator Load(string endpoint, Action<CatalogResponse, string> completed)
        {
            if (!string.IsNullOrEmpty(endpoint))
            {
                using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
                {
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        CatalogResponse remote = JsonUtility.FromJson<CatalogResponse>(request.downloadHandler.text);
                        completed?.Invoke(remote, "HTTP JSON API");
                        yield break;
                    }
                }
            }

            TextAsset local = Resources.Load<TextAsset>("catalog");
            if (local != null)
            {
                CatalogResponse fallback = JsonUtility.FromJson<CatalogResponse>(local.text);
                completed?.Invoke(fallback, "local JSON fallback");
                yield break;
            }

            completed?.Invoke(new CatalogResponse { products = new ProductRecord[0] }, "empty catalog");
        }

        public IEnumerator LoadRecommendations(string endpoint, string profile, Action<RecommendationResponse, string> completed)
        {
            string requestEndpoint = endpoint + (endpoint.Contains("?") ? "&" : "?") + "profile=" + UnityWebRequest.EscapeURL(profile);
            using (UnityWebRequest request = UnityWebRequest.Get(requestEndpoint))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    RecommendationResponse remote = JsonUtility.FromJson<RecommendationResponse>(request.downloadHandler.text);
                    completed?.Invoke(remote, "HTTP recommendation API");
                    yield break;
                }
            }

            TextAsset local = Resources.Load<TextAsset>("catalog");
            CatalogResponse fallback = local == null
                ? new CatalogResponse { products = new ProductRecord[0] }
                : JsonUtility.FromJson<CatalogResponse>(local.text);
            completed?.Invoke(new RecommendationResponse
            {
                profile = profile,
                rationale = "Local deterministic fallback; the client keeps the same service contract when the API is offline.",
                model = "local-fallback",
                products = fallback.products
            }, "local recommendation fallback");
        }
    }
}
