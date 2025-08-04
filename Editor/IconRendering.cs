using System;
using System.Numerics;
using Editor;
using IconRenderer;
using Sandbox;

internal class IconRendering : SceneRenderingWidget
{
	public readonly CameraComponent camera;
	public readonly SkinnedModelRenderer model;
	public readonly DirectionalLight light;
	public readonly SpriteRenderer mockBackground;

	private float hoverTime;

	public IconRendering( string modelName, string background ) : base( null )
	{
		Size = 512;

		Scene = Scene.CreateEditorScene();

		using ( Scene.Push() )
		{
			{
				camera = new GameObject( true, "camera" ).GetOrAddComponent<CameraComponent>( false );
				camera.BackgroundColor = Color.Black;
				camera.Enabled = true;
			}
			{
				light = new GameObject( true, "light" ).GetOrAddComponent<DirectionalLight>( false );
				light.LightColor = Color.White;
				light.Enabled = true;
			}
			{
				model = new GameObject( true, "model" ).GetOrAddComponent<SkinnedModelRenderer>( false );
				model.Model = Model.Load( modelName ?? "models/error.vmdl" );
				model.Enabled = true;
			}
			{
				mockBackground = new GameObject( true, "mockBackground" ).GetOrAddComponent<SpriteRenderer>( false );
				mockBackground.Texture = Texture.Load( background ?? "textures/white.png" );
				mockBackground.Size = new Vector2( 512, 512 );
				mockBackground.LocalPosition = new Vector3( -256, 0, 0 );
				mockBackground.Enabled = true;
			}
		}
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		Scene?.Destroy();
		Scene = null;
	}

	public override void PreFrame()
	{
		Scene.EditorTick( RealTime.Now, RealTime.Delta );
	}
}
