using System;

namespace ElysiumMall
{
    [Serializable]
    public class CatalogResponse
    {
        public ProductRecord[] products;
    }

    [Serializable]
    public class ProductRecord
    {
        public string id;
        public string name;
        public string vendor;
        public string category;
        public string description;
        public float price;
        public string spriteKind;
    }

    [Serializable]
    public class RecommendationResponse
    {
        public string profile;
        public string rationale;
        public string model;
        public ProductRecord[] products;
    }
}
