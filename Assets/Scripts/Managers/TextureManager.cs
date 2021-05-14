using System.Collections.Generic;
using System.IO;
using System.Linq;
using Globals;
using UnityEngine;

public class TextureManager
    {
        #region Singleton
        
        private static TextureManager _instance;
        public static TextureManager Instance => _instance ??= new TextureManager();
        
        #endregion
        
        private string _basePath = GV.TexturesPath;
        private Dictionary<string, Dictionary<string, Texture2D>> _texturesDict;

        private TextureManager() { }

        public void Initialize(string path = "")
        {
            if (!string.IsNullOrEmpty(path))
            {
                SetBasePath(path);
                SetTexturesDict();
            }
            else
            {
                SetTexturesDict();
            }
        }

        private void SetBasePath(string path)
        {
            _basePath = GV.TexturesPath + path;
            if(!_basePath.EndsWith("/"))
                    _basePath += "/";
        }

        private void SetTexturesDict()
        {
            _texturesDict = new Dictionary<string, Dictionary<string, Texture2D>>();
            IEnumerable<string> subDirs = new DirectoryInfo(_basePath).GetDirectories().Select(subDir => subDir.Name);
            
            foreach(var dir in subDirs)
                _texturesDict.Add(dir, GetTexturesDict(_basePath + dir));
        }
        
        private Dictionary<string, Texture2D> GetTexturesDict(string texturesPath)
        {
            Dictionary<string, Texture2D> dict = new Dictionary<string, Texture2D>();

            string[] texturePaths = Directory.GetFiles(texturesPath, $"*{GV.TextureExt}", SearchOption.AllDirectories);
            foreach (var texturePath in texturePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(texturePath);
                if (!dict.ContainsKey(fileName.ToLower()))
                    dict.Add(Path.GetFileNameWithoutExtension(texturePath.ToLower()), Resources.Load<Texture2D>(GetResourcePath(texturePath)));
            }
            return dict;
        }
        
        private static string GetResourcePath(string texturePath)
        {
            return texturePath.Replace(GV.ApplicationResourcePath, "").Replace("\\", "/").Replace(GV.TextureExt, "");
        }

        public Texture2D GetFirst(string name)
        {
            return (from td in _texturesDict
                    from texture in td.Value
                    where texture.Key == name.ToLower()
                    select texture.Value).FirstOrDefault();
        }
        
        public Texture2D GetFirstLike(string name)
        {
            return (from td in _texturesDict
                    from texture in td.Value
                    where texture.Key.Contains(name.ToLower())
                    select texture.Value).FirstOrDefault();
        }
    }
    