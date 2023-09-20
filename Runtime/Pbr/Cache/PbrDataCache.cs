using System;
using System.IO;
using UltraLiteDB;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture.Pbr.Cache
{
    public static class PbrDataCache
    {
        static readonly string k_FileStreamPath = $"{Application.persistentDataPath}/{k_DatabaseName}";
        const string k_DatabaseName = "PbrDataCache.db";
        const string k_ArtifactCollectionName = "PbrData";

        public static bool IsInCache(Artifact albedoArtifact)
        {
            using var fs = new FileStream(k_FileStreamPath, FileMode.OpenOrCreate);
            using var db = new UltraLiteDatabase(fs);
            var collection = db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);

            var all = collection.FindAll();

            var query = Query.Where("AlbedoGuid", value => value.AsString == albedoArtifact.Guid);
            return collection.FindOne(query) != null;
        }

        public static void Write(ProcessedPbrMaterialData materialData)
        {
            using var fs = new FileStream(k_FileStreamPath, FileMode.OpenOrCreate);
            using var db = new UltraLiteDatabase(fs);
            var collection = db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);

            var query = Query.Where("AlbedoGuid", value => value.AsString == materialData.BaseMap.Guid);
            var artifactObject = collection.FindOne(query) ?? new PbrDatabaseObject(materialData);

            collection.Upsert(artifactObject);
        }

        public static ProcessedPbrMaterialData GetPbrMaterialData(Artifact albedoArtifact)
        {
            using var fs = new FileStream(k_FileStreamPath, FileMode.OpenOrCreate);
            using var db = new UltraLiteDatabase(fs);
            var collection = db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);

            var query = Query.Where("AlbedoGuid", value => value.AsString == albedoArtifact.Guid);
            var data = collection.FindOne(query);

            var processedPbrData = new ProcessedPbrMaterialData()
            {
                BaseMap = new ImageArtifact(data.AlbedoGuid, uint.MinValue),
                NormalMap = new ImageArtifact(data.NormalGuid, uint.MinValue),
                MetallicMap = new ImageArtifact(data.MetallicGuid, uint.MinValue),
                RoughnessMap = new ImageArtifact(data.RoughnessGuid, uint.MinValue),
                HeightmapMap = new ImageArtifact(data.HeightGuid, uint.MinValue),
                DiffuseMap = new ImageArtifact(data.DiffuseGuid, uint.MinValue),
            };

            processedPbrData.BaseMapPNGData = ArtifactCache.ReadRawData(processedPbrData.BaseMap);
            processedPbrData.NormalMapPNGData = ArtifactCache.ReadRawData(processedPbrData.NormalMap);
            processedPbrData.MetallicMapPNGData = ArtifactCache.ReadRawData(processedPbrData.MetallicMap);
            processedPbrData.RoughnessMapPNGData = ArtifactCache.ReadRawData(processedPbrData.RoughnessMap);
            processedPbrData.HeightmapPNGData = ArtifactCache.ReadRawData(processedPbrData.HeightmapMap);
            processedPbrData.DiffuseMapPNGData = ArtifactCache.ReadRawData(processedPbrData.DiffuseMap);

            return processedPbrData;
        }
    }
}
