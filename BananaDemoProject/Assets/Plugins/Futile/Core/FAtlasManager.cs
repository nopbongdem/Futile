using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FAtlasManager
{
	static private int _nextAtlasIndex;
	
	private List<FAtlas> _atlases = new List<FAtlas>();
	
	private Dictionary<string, FAtlasElement> _allElementsByName = new Dictionary<string, FAtlasElement>();
	
	private List<FFont> _fonts = new List<FFont>();
	private Dictionary<string,FFont> _fontsByName = new Dictionary<string, FFont>();
	
	public FAtlasManager () //new DAtlasManager() called by Futile
	{
		
	}
	
	public bool DoesContainAtlas(string name)
	{
		int atlasCount = _atlases.Count;
		for(int a = 0; a<atlasCount; ++a)
		{
			if(_atlases[a].name == name) return true;
		}
		return false;
	}

	public void LoadAtlasFromTexture (string name, Texture texture)
	{
		if(DoesContainAtlas(name)) return; //we already have it, don't load it again
		
		FAtlas atlas = new FAtlas(name, texture, _nextAtlasIndex++);
		
		AddAtlas(atlas);
	}
	
	public void LoadAtlasFromTexture (string name, string dataPath, Texture texture)
	{
		if(DoesContainAtlas(name)) return; //we already have it, don't load it again
		
		FAtlas atlas = new FAtlas(name, dataPath, texture, _nextAtlasIndex++);
		
		AddAtlas(atlas);
	}
	
	public void ActuallyLoadAtlasOrImage(string name, string imagePath, string dataPath)
	{
		if(DoesContainAtlas(name)) return; //we already have it, don't load it again
		
		//if dataPath is empty, load it as a single image
		bool isSingleImage = (dataPath == "");
		
		FAtlas atlas = new FAtlas(name, imagePath, dataPath, _nextAtlasIndex++, isSingleImage);
		
		AddAtlas(atlas);
	}
	
	private void AddAtlas(FAtlas atlas)
	{
		int elementCount = atlas.elements.Count;
		for(int e = 0; e<elementCount; ++e)
		{
			FAtlasElement element = atlas.elements[e];
			
			element.atlas = atlas;
			element.atlasIndex = atlas.index;
			
			if(_allElementsByName.ContainsKey(element.name))
			{
				throw new FutileException("Duplicate element name found! All element names must be unique!");	
			}
			else 
			{
				_allElementsByName.Add (element.name, element);
			}
		}
		
		_atlases.Add(atlas); 
	}
	
	public void LoadAtlas(string atlasPath)
	{
		ActuallyLoadAtlasOrImage(atlasPath, atlasPath+Futile.resourceSuffix, atlasPath+Futile.resourceSuffix);
	}
	
	public void LoadAtlas(string atlasPath, bool isSpecialPNG) //load a special image with the suffix "_image.bytes"
	{
		string filePath = atlasPath+Futile.resourceSuffix+"_image";
		
		TextAsset text = Resources.Load (filePath, typeof(TextAsset)) as TextAsset;
		
		Texture2D texture = new Texture2D(0,0,TextureFormat.ARGB32,false);
		
		texture.LoadImage(text.bytes);
		
		Futile.atlasManager.LoadAtlasFromTexture(atlasPath,atlasPath+Futile.resourceSuffix, texture);
	}

	public void LoadImage(string imagePath)
	{
		ActuallyLoadAtlasOrImage(imagePath, imagePath+Futile.resourceSuffix,"");
	}
	
	public void ActuallyUnloadAtlasOrImage(string name)
	{
		bool wasAtlasRemoved = false;
		
		int atlasCount = _atlases.Count;
		
		for(int a = atlasCount-1; a>=0; a--) //reverse order so deletions ain't no thang
		{
			FAtlas atlas = _atlases[a];
			
			if(atlas.name == name)
			{
				int elementCount = atlas.elements.Count;
				
				for(int e = 0; e<elementCount; e++)
				{
					_allElementsByName.Remove(atlas.elements[e].name);	
				}
				
				atlas.Unload();
				_atlases.RemoveAt(a);
				
				wasAtlasRemoved = true;
			}
		}
		
		if(wasAtlasRemoved)
		{
			Futile.stage.renderer.Clear();
			Resources.UnloadUnusedAssets();
		}
	}
	
	
	public void UnloadAtlas(string atlasPath)
	{
		ActuallyUnloadAtlasOrImage(atlasPath);
	}
	
	public void UnloadImage(string imagePath)
	{
		ActuallyUnloadAtlasOrImage(imagePath);	
	}

	public FAtlasElement GetElementWithName (string elementName)
	{
		if(_allElementsByName.ContainsKey(elementName))
		{
			return _allElementsByName[elementName];
		}
		throw new FutileException("Couldn't find element named '"+elementName+"'");
	}
	
	public FFont GetFontWithName(string fontName)
	{
		if(!_fontsByName.ContainsKey(fontName))
		{
			throw new FutileException("Couldn't find font named '"+fontName+"'");
		}
		return _fontsByName[fontName];	
	}

	public void LoadFont (string name, string elementName, string configPath, float offsetX, float offsetY)
	{
		LoadFont (name,elementName,configPath, offsetX, offsetY, new FTextParams());
	}
	
	public void LoadFont (string name, string elementName, string configPath, float offsetX, float offsetY, FTextParams textParams)
	{
		FAtlasElement element = GetElementWithName(elementName);
		FFont font = new FFont(name,element,configPath, offsetX, offsetY, textParams);
	
		_fonts.Add(font);
		_fontsByName.Add (name, font);
	}
}


