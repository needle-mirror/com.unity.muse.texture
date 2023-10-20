using System;
using System.IO;
using Unity.Muse.Common;
using UnityEngine;

#if UNITY_WEBGL
using UltraLiteDB;
using Query = UltraLiteDB.Query;
#else
using LiteDB;
#endif

namespace Unity.Muse.Texture.Pbr.Cache
{
    internal static class PbrDataCache
    {
        static readonly string k_FileStreamPath = $"{Application.persistentDataPath}/{k_DatabaseName}";
        const string k_DatabaseName = "PbrDataCache.db";
        const string k_ArtifactCollectionName = "PbrData";

#if UNITY_WEBGL
        static FileStream s_Fs;
        static UltraLiteDatabase s_Db;

        static UltraLiteDatabase Db
        {
            get
            {
                try
                {
                    s_Fs = new FileStream(k_FileStreamPath, FileMode.OpenOrCreate);
                    s_Db = new UltraLiteDatabase(s_Fs);
                    var collection = s_Db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);
                    collection.EnsureIndex("AlbedoGuid");
                    return s_Db;
                }
                catch (UltraLiteException)
                {
                    s_Fs?.Dispose();
                    File.Delete(k_FileStreamPath);
                    return Db;
                }
            }
        }
        static PbrDatabaseObject FindOne(string albedoGuid)
        {
            var collection = Db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);
            var query = Query.Where("AlbedoGuid", value => value.AsString == albedoGuid);
            var result = collection.FindOne(query);
            s_Db.Dispose();
            s_Fs.Dispose();
            return result;
        }

        static void Upsert(PbrDatabaseObject artifactObject)
        {
            var collection = Db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);
            collection.Upsert(artifactObject);
            s_Db.Dispose();
            s_Fs.Dispose();
        }
#else
        static FileStream s_Fs;
        static LiteDatabase s_Db;
        static LiteDatabase Db
        {
            get
            {
                try
                {
                    s_Fs = new FileStream(k_FileStreamPath, FileMode.OpenOrCreate);
                    s_Db = new LiteDatabase(s_Fs);
                    var collection = s_Db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);
                    collection.EnsureIndex("AlbedoGuid");
                    return s_Db;
                }
                catch (LiteException)
                {
                    s_Fs?.Dispose();
                    File.Delete(k_FileStreamPath);
                    return Db;
                }
            }
        }

        static PbrDatabaseObject FindOne(string albedoGuid)
        {
            var collection = Db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);
            var result = collection.FindOne(item => item.AlbedoGuid == albedoGuid);
            s_Db.Dispose();
            s_Fs.Dispose();
            return result;
        }

        static void Upsert(PbrDatabaseObject artifactObject)
        {
            var collection = Db.GetCollection<PbrDatabaseObject>(k_ArtifactCollectionName);
            collection.Upsert(artifactObject);
            s_Db.Dispose();
            s_Fs.Dispose();
        }
#endif

        public static bool IsInCache(Artifact albedoArtifact)
        {
            return FindOne(albedoArtifact.Guid) != null;
        }

        public static void Write(ProcessedPbrMaterialData materialData)
        {
            var artifactObject = FindOne(materialData.BaseMap.Guid) ?? new PbrDatabaseObject(materialData);
            Upsert(artifactObject);
        }

        public static ProcessedPbrMaterialData GetPbrMaterialData(Artifact albedoArtifact)
        {
            var data = FindOne(albedoArtifact.Guid);

            var processedPbrData = new ProcessedPbrMaterialData()
            {
                BaseMap = new ImageArtifact(data.AlbedoGuid, uint.MinValue),
                NormalMap = new ImageArtifact(data.NormalGuid, uint.MinValue),
                MetallicMap = new ImageArtifact(data.MetallicGuid, uint.MinValue),
                SmoothnessMap = new ImageArtifact(data.SmoothnessGuid, uint.MinValue),
                HeightmapMap = new ImageArtifact(data.HeightGuid, uint.MinValue),
                DiffuseMap = new ImageArtifact(data.DiffuseGuid, uint.MinValue),
            };

            processedPbrData.BaseMapPNGData = ArtifactCache.ReadRawData(processedPbrData.BaseMap);
            processedPbrData.NormalMapPNGData = ArtifactCache.ReadRawData(processedPbrData.NormalMap);
            processedPbrData.MetallicMapPNGData = ArtifactCache.ReadRawData(processedPbrData.MetallicMap);
            processedPbrData.SmoothnessMapPNGData = ArtifactCache.ReadRawData(processedPbrData.SmoothnessMap);
            processedPbrData.HeightmapPNGData = ArtifactCache.ReadRawData(processedPbrData.HeightmapMap);
            processedPbrData.DiffuseMapPNGData = ArtifactCache.ReadRawData(processedPbrData.DiffuseMap);

            return processedPbrData;
        }
    }
}
