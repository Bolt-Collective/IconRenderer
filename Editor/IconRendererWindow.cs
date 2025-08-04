using System;
using System.IO;
using System.Numerics;
using Editor;
using Sandbox;
using FileSystem = Editor.FileSystem;

namespace IconRenderer;


[EditorApp( "Icon Renderer", "texture", "Make icons" )]
public class IconRendererWindow : GraphicsView
{
	public static IconRendererWindow Instance { get; private set; }
	
	public IconConfiguration Configuration { get; set; }
	
	private IconRendering renderer;

	public IconRendererWindow( )
	{
		Instance = this;
		Configuration = new IconConfiguration();

		WindowTitle = "Icon Renderer";
		SetWindowIcon( "texture" );
		Size = new Vector2( 900, 1000 );
		
		Layout = Layout.Column();
		
		Layout.Margin = 4;
		Layout.Spacing = 12;
		
		CreateUI();
	}
	
	void CreateUI()
	{
		if (Configuration == null)
			throw new Exception("IconConfiguration is null");
			
		var so = Configuration?.GetSerialized();
		var ps = new ControlSheet();
		
		if (!so.IsValid())
			throw new Exception("Config serialized property is not valid");
		
		if (!ps.IsValid())
			throw new Exception("ControlSheet is not valid");
		
		SetStyles( "color: white; font-weight: 500;" );
		{
			ps.AddObject( so );
			Layout.Add( ps );
			
			var materialGroup = Layout.Add( new LineEdit(), 1 );
			materialGroup.PlaceholderText = "Material Group...";
			materialGroup.TextEdited += ( text ) =>
			{
				renderer.model.MaterialGroup = text;
			};

			var configButtons = Layout.AddRow( );
			configButtons.Spacing = 8;
			
			configButtons.Add( new Button( this )
			{
				Text = "Save Configuration",
				Clicked = () =>
				{
					var fd = EditorUtility.SaveFileDialog( "Save Icon Configuration", ".json",
						Project.Current?.GetAssetsPath() );
					if ( fd == null )
					{
						return;
					}
					
					Configuration?.Save(fd);
					EditorUtility.DisplayDialog( "Saved", $"Icon configuration saved to {fd}" );
				}
			}, 1 );
			configButtons.Add( new Button( this )
			{
				Text = "Load Configuration",
				Clicked = () =>
				{
					var fd = EditorUtility.OpenFileDialog("Load Icon Configuration", ".json", Project.Current?.GetAssetsPath() );

					if ( fd == null )
					{
						EditorUtility.DisplayDialog( "Error", "No .json config specified" );
						return;
					}
					
					var config = Configuration?.Load( fd );
					Configuration = config;
					
					Rebuild();
				}
			}, 1 );
		}

		Layout.AddSpacingCell( 4 );
		{
			// Scene
			renderer = new IconRendering( Configuration?.Model, Configuration?.ImageBackground );
			Layout.Add( renderer, 1 );
		}
		
		bool saveToIcons = false;

		Layout.AddSpacingCell( 4 );
		{
			var checkbox = Layout.Add( new Checkbox( this ) );
			checkbox.Text = "Save to icons/";
			checkbox.Toggled += () =>
			{
				if ( checkbox.Value )
				{
					saveToIcons = true;
				}
				else
				{
					saveToIcons = false;
				}
			};
			// Save Button
			var button = Layout.Add( new global::Editor.Button( this )
			{
				Text = "Save Icon",
				Clicked = () =>
				{
					var pixmap = new Pixmap( Configuration.RenderResolution, Configuration.RenderResolution ); 
				
					if ( saveToIcons )
					{
						var baseIconPath = Path.Combine(Project.Current.GetAssetsPath(), "icons");

						var modelPath = Configuration.Model.Replace(".vmdl", ""); // remove extension if needed
						var modelDir = Path.Combine(baseIconPath, Path.GetDirectoryName(modelPath) ?? "");

						Directory.CreateDirectory(modelDir);

						var fileName = $"{Path.GetFileName(modelPath)}.vmdl_c.png";
						var fullPath = Path.Combine(modelDir, fileName);

						renderer.camera.RenderToPixmap(pixmap);
						pixmap.SavePng(fullPath);

						Log.Info($"Icon saved to {fullPath}");
					}
					else
					{
						var fd = EditorUtility.SaveFileDialog( "Save Icon", ".png", Project.Current?.GetAssetsPath() );
					
						if (fd == null)
						{
							return;
						}
					
						renderer.camera.RenderToPixmap( pixmap );
						pixmap.SavePng( fd );
					}
				}
			}, 1 );
		}
	}

	void Rebuild()
	{
		Instance = this;
		renderer?.Destroy();
		Layout?.Clear( true );
		CreateUI();
	}

	[EditorEvent.Hotload]
	void OnHotload()
	{
		Rebuild();
	}

	[EditorEvent.Frame]
	void OnFrame()
	{
		if ( !renderer.IsValid() )
			return;
		if ( !renderer.model.IsValid() )
			return;
		if ( !renderer.camera.IsValid() )
			return;
		
		if (Configuration == null)
			return;
		
		renderer.camera.BackgroundColor = Configuration.Color;
		renderer.model.Model = Model.Load(Configuration.Model);
		renderer.camera.FitModel( renderer.model );
		renderer.camera.FieldOfView = Configuration.FOV;
		
		renderer.mockBackground.Texture = Texture.Load( Configuration.ImageBackground );
		renderer.mockBackground.Size = Configuration.BackgroundSize;
		renderer.mockBackground.WorldPosition = new Vector3( Configuration.BackgroundPosition.x, Configuration.BackgroundPosition.y, Configuration.BackgroundPosition.z );
		renderer.model.WorldPosition = Configuration.ModelPosition;
		renderer.model.WorldRotation = Configuration.ModelRotation;
		renderer.light.WorldRotation = new Angles( -40, 180, 0 );
		renderer.Update();

	}
}

