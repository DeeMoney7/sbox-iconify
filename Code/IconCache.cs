using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

namespace Iconify;

/// <summary>
/// Fetches icons from the Iconify API and caches them as textures.
/// Handles disk cache and in-memory cache with proper null safety.
/// </summary>
public static class IconCache
{
	private static readonly Dictionary<string, Texture> MemoryCache = new();
	private static BaseFileSystem _diskCache;
	private static bool _diskCacheReady;

	private static BaseFileSystem DiskCache
	{
		get
		{
			if ( !_diskCacheReady )
			{
				_diskCacheReady = true;
				try
				{
					if ( FileSystem.Data is not null )
					{
						FileSystem.Data.CreateDirectory( "iconify_cache" );
						_diskCache = FileSystem.Data.CreateSubSystem( "iconify_cache" );
					}
				}
				catch ( Exception e )
				{
					Log.Warning( $"[Iconify] Could not create disk cache: {e.Message}" );
					_diskCache = null;
				}
			}
			return _diskCache;
		}
	}

	/// <summary>
	/// Get an icon texture from cache or fetch from the Iconify API.
	/// </summary>
	public static async Task<Texture> GetOrFetch( string prefix, string name, string color, int size )
	{
		var cacheKey = $"{prefix}_{name}_{color}_{size}";

		// Memory cache
		if ( MemoryCache.TryGetValue( cacheKey, out var cached ) && cached.IsValid() )
			return cached;

		// Disk cache
		var diskPath = $"{cacheKey}.png";
		if ( DiskCache is not null && DiskCache.FileExists( diskPath ) )
		{
			try
			{
				var tex = Texture.Load( DiskCache, diskPath );
				if ( tex is not null && tex.IsValid() )
				{
					MemoryCache[cacheKey] = tex;
					return tex;
				}
			}
			catch { /* disk cache corrupted, re-fetch */ }
		}

		// Fetch from API
		var texture = await FetchFromApi( prefix, name, color, size );
		if ( texture is not null )
		{
			MemoryCache[cacheKey] = texture;
		}

		return texture;
	}

	private static async Task<Texture> FetchFromApi( string prefix, string name, string color, int size )
	{
		var encodedColor = Uri.EscapeDataString( color ?? "white" );
		var url = $"https://api.iconify.design/{prefix}/{name}.svg?color={encodedColor}&width={size}&height={size}";

		try
		{
			var http = new Http( new Uri( url ) );
			var svgData = await http.GetBytesAsync();

			if ( svgData is null || svgData.Length == 0 )
			{
				Log.Warning( $"[Iconify] Empty response for {prefix}:{name}" );
				return null;
			}

			// S&Box can create textures from SVG data
			var texture = Texture.Load( svgData );

			if ( texture is not null && texture.IsValid() )
			{
				// Save to disk cache
				SaveToDiskCache( $"{prefix}_{name}_{color}_{size}.png", svgData );
			}

			return texture;
		}
		catch ( Exception e )
		{
			Log.Warning( $"[Iconify] API fetch failed for {prefix}:{name}: {e.Message}" );
			return null;
		}
	}

	private static void SaveToDiskCache( string path, byte[] data )
	{
		try
		{
			DiskCache?.WriteAllBytes( path, data );
		}
		catch { /* non-critical, just won't be cached */ }
	}
}
