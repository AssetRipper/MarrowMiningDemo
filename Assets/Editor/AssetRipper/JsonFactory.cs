using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Primitives;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace AssetRipper
{
    public static class JsonFactory
    {
        [MenuItem("AssetRipper/Log Packages")]
        public static void LogPackages()
        {
            var packages = PackageInfo.GetAllRegisteredPackages();
            UnityEngine.Debug.Log($"Found {packages.Length} packages");
            foreach (var package in packages)
            {
                UnityEngine.Debug.Log($"Name: {package.name} Version: {GetVersion(package)} Asset Path: {package.assetPath}");
            }
        }

        [MenuItem("AssetRipper/Log Asset Paths")]
        public static void LogAssetPaths()
        {
            var assets = AssetDatabase.GetAllAssetPaths();
            UnityEngine.Debug.Log($"Found {assets.Length} assets");
            foreach (var asset in assets)
            {
                UnityEngine.Debug.Log($"Asset Path: {asset}");
            }
        }

        [MenuItem("AssetRipper/Create Json Files")]
        public static void CreateJsonFiles()
        {
            Dictionary<string, UnityPackageData> dictionary = new();
            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                UnityPackageData packageData = new UnityPackageData(package.name, GetVersion(package), true);
                dictionary.Add(package.assetPath, packageData);
            }
            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                if (dictionary.TryGetPackage(path, out UnityPackageData packageData))
                {
                    if (path.EndsWith(".dll", System.StringComparison.Ordinal))
                    {
                        if (!packageData.Assemblies.TryAdd(Path.GetFileNameWithoutExtension(path), GetGuid(path)))
                        {
                        }
                        continue;
                    }

                    foreach (var asset in assets)
                    {
                        if (asset is UnityEditor.MonoScript monoScript)
                        {
                            var type = monoScript.GetClass();
                            if (type == null)
                            {
                            }
                            else if (!typeof(UnityEngine.Object).IsAssignableFrom(type))
                            {
                            }
                            else
                            {
                                string name = type.Assembly.GetName().Name;
                                packageData.Scripts.GetOrCreate(name).Add(type.FullName, GetGuid(path));
                            }
                        }
                        else if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guidString, out long localId))
                        {
                            UnityGuid guid = UnityGuid.Parse(guidString);
                            AssetType type = AssetDatabase.IsNativeAsset(asset) ? AssetType.Serialized : AssetType.Meta;
                            PPtr pptr = new PPtr(localId, guid, type);
                            if (asset is UnityEngine.Shader shader)
                            {
                                packageData.Assets.Add(new Shader(shader.name, shader.GetPropertyNames()), pptr);
                            }
                            else if (asset is UnityEngine.TextAsset textAsset)
                            {
                                packageData.Assets.Add(new TextAsset(textAsset.name, textAsset.bytes), pptr);
                            }
                            else if (asset is UnityEngine.Mesh mesh)
                            {
                                packageData.Assets.Add(new Mesh(mesh.name, mesh.vertexCount, mesh.subMeshCount), pptr);
                            }
                            else if (asset is UnityEngine.Cubemap cubemap)
                            {
                                packageData.Assets.Add(new Cubemap(cubemap.name, cubemap.width, cubemap.height), pptr);
                            }
                            else if (asset is UnityEngine.Texture2D texture2D)
                            {
                                packageData.Assets.Add(new Texture2D(texture2D.name, texture2D.width, texture2D.height), pptr);
                            }
                            else if (asset is UnityEngine.AudioClip audioClip)
                            {
                                packageData.Assets.Add(new AudioClip(audioClip.name, audioClip.channels, audioClip.frequency, audioClip.length), pptr);
                            }
                            else if (asset is UnityEngine.Font font)
                            {
                                packageData.Assets.Add(GenericNamedObject.CreateFont(font.name), pptr);
                            }
                            else if (asset is UnityEngine.ComputeShader computeShader)
                            {
                                packageData.Assets.Add(GenericNamedObject.CreateComputeShader(computeShader.name), pptr);
                            }
                            else if (asset is UnityEngine.Material material)
                            {
                                packageData.Assets.Add(new Material(material.name, material.shader != null ? material.shader.name : null), pptr);
                            }
                            else if (asset is UnityEngine.Sprite sprite)
                            {
                                packageData.Assets.Add(new Sprite(sprite.name, sprite.texture != null ? sprite.texture.name : null), pptr);
                            }
                        }
                        else
                        {
                        }
                    }
                }
                else
                {
                }
            }

            const string OutputFolder = "AssetRipper/PackageData/";
            if (Directory.Exists(OutputFolder))
            {
                Directory.Delete(OutputFolder, true);
            }
            Directory.CreateDirectory(OutputFolder);
            foreach (var packageData in dictionary.Values)
            {
                string json = packageData.ToJson();
                string path = $"{OutputFolder}{packageData.Name}.json";
                File.WriteAllText(path, json);
            }
            UnityEngine.Debug.Log($"Wrote data for {dictionary.Count} packages");
        }

        private static string GetVersion(PackageInfo package)
        {
            return package.packageId.Substring(package.name.Length + 1);
            // +1 for the @
        }

        private static string[] GetPropertyNames(this UnityEngine.Shader shader)
        {
            string[] propertyNames = new string[shader.GetPropertyCount()];
            for (int i = 0; i < propertyNames.Length; i++)
            {
                propertyNames[i] = shader.GetPropertyName(i);
            }
            return propertyNames;
        }

        private static UnityGuid GetGuid(string path)
        {
            return UnityGuid.Parse(AssetDatabase.AssetPathToGUID(path));
        }

        private static bool TryGetPackage(this Dictionary<string, UnityPackageData> dictionary, string path, out UnityPackageData packageData)
        {
            foreach (var pair in dictionary)
            {
                if (path.StartsWith(pair.Key, System.StringComparison.Ordinal))
                {
                    packageData = pair.Value;
                    return true;
                }
            }
            packageData = default;
            return false;
        }

        private static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }
            else
            {
                value = new TValue();
                dictionary.Add(key, value);
                return value;
            }
        }

        private static void Add<TKey, TValue>(this List<KeyValuePair<TKey, TValue>> list, TKey key, TValue value)
        {
            list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
