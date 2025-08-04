using System.IO;
using System.Text.Json;
using Editor;
using Sandbox;
using Sandbox.Mounting;
using FileSystem = Editor.FileSystem;

namespace IconRenderer;

public class IconConfiguration
{
	[Group("Model"), ResourceType("vmdl")] public string Model { get; set; } = "models/citizen/citizen.vmdl";
	public Color Color { get; set; } = Color.White;
	[Group("Model")] public Angles ModelRotation { get; set; } = Angles.Zero;
	[Group("Model")] public Vector3 ModelPosition { get; set; } = Vector3.Zero;
	[Group("Background")] public Vector2 BackgroundSize { get; set; } = new Vector2( 512, 512 );
	[Group("Background")] public Vector3 BackgroundPosition { get; set; } = new Vector3( -256, 0, 0 );
	
	public int RenderResolution { get; set; } = 512;
	
	public float FOV { get; set; } = 90f;
	[TextureImagePath, Group("Background")] public string ImageBackground { get; set; } = string.Empty;

	public void Save(string path)
	{
		var json = JsonSerializer.Serialize( this );
		System.IO.File.WriteAllText( path, json );
	}

	public IconConfiguration Load(string iconConfigPath)
	{
		var json = File.ReadAllText( iconConfigPath );
		return JsonSerializer.Deserialize<IconConfiguration>( json );
	}
}
