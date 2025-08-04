using Sandbox;
using System;

namespace IconRenderer;

public static class SceneCameraExtensions
{
	public static void FitModel( this CameraComponent camera, ModelRenderer model )
	{
		if (!model.IsValid())
			return;
		
		var bounds = model.Model.Bounds;
		var max = bounds.Size;
		var radius = MathF.Max( max.x, MathF.Max( max.y, max.z ) );
		var dist = radius / MathF.Sin( camera.FieldOfView.DegreeToRadian() );

		var viewDirection = Vector3.Forward;
		var pos = viewDirection * dist + bounds.Center;

		camera.WorldPosition = pos;
		camera.WorldRotation = global::Rotation.LookAt( bounds.Center - camera.WorldPosition ).RotateAroundAxis( -viewDirection, 90 );
	}
}
