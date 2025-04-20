using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CsTranslator.Models;

namespace CsTranslator.Helpers
{
    public static class TranslationCache
    {
        private static readonly string CacheFilePath =
            $@"{Properties.Settings.Default.Path}\game\csgo\translationCache.json";
        private static Dictionary<string, Translation> _cache;
        private static readonly int MaxCacheSize = 10000;

        static TranslationCache()
        {
            LoadCache();
        }

        private static void LoadCache()
        {
            if (!File.Exists(CacheFilePath))
            {
                _cache = new Dictionary<string, Translation>();
                return;
            }

            try
            {
                var json = File.ReadAllText(CacheFilePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, Translation>>(json)
                         ?? new Dictionary<string, Translation>();
            }
            catch
            {
                _cache = new Dictionary<string, Translation>();
            }
        }

        public static void SaveCache()
        {
            EnsureCacheSize();

            var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(CacheFilePath, json);
        }

        private static void EnsureCacheSize()
        {
            if (_cache.Count > MaxCacheSize)
            {
                var firstKey = new List<string>(_cache.Keys)[0];
                _cache.Remove(firstKey);
                Console.WriteLine("[CACHE ROTATED] Oldest entry removed.");
            }
        }

        public static bool TryGet(string key, out Translation translation)
        {
            bool found = _cache.TryGetValue(key, out translation);
            Console.WriteLine(found
                ? $"[CACHE HIT] {key} => {translation?.Message}"
                : $"[CACHE MISS] {key}");
            return found;
        }

        public static void Add(string key, Translation translation)
        {
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = translation;
                Console.WriteLine($"[CACHE ADD] {key} => {translation?.Message}");
                SaveCache();
            }
        }
    }
}