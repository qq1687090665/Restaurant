Shader "Toony Colors Pro 2/Examples/Default/Comic Book" {
	Properties {
		[TCP2HeaderHelp(BASE, Base Properties)] _Color ("Color", Vector) = (1,1,1,1)
		_HColor ("Highlight Color", Vector) = (0.785,0.785,0.785,1)
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		[TCP2Separator] [TCP2Header(RAMP SETTINGS)] _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
		_RampSmooth ("Ramp Smoothing", Range(0.001, 1)) = 0.1
		[TCP2Separator] [TCP2HeaderHelp(SPECULAR, Specular)] _SpecColor ("Specular Color", Vector) = (0.5,0.5,0.5,1)
		_Smoothness ("Size", Float) = 0.2
		_SpecSmooth ("Smoothness", Range(0, 1)) = 0.05
		[TCP2Separator] [TCP2HeaderHelp(RIM, Rim)] _RimColor ("Rim Color", Vector) = (0.8,0.8,0.8,0.6)
		_RimMin ("Rim Min", Range(0, 2)) = 0.5
		_RimMax ("Rim Max", Range(0, 2)) = 1
		[TCP2Separator] [TCP2HeaderHelp(SKETCH, Sketch)] _SketchTex ("Sketch (Alpha)", 2D) = "white" {}
		_SketchColor ("Sketch Color (RGB)", Vector) = (0,0,0,1)
		_SketchHalftoneMin ("Sketch Halftone Min", Range(0, 1)) = 0.2
		_SketchHalftoneMax ("Sketch Halftone Max", Range(0, 1)) = 1
		[TCP2Separator] [TCP2HeaderHelp(OUTLINE, Outline)] _OutlineColor ("Outline Color", Vector) = (0.2,0.2,0.2,1)
		_Outline ("Outline Width", Float) = 1
		[Toggle(TCP2_OUTLINE_TEXTURED)] _EnableTexturedOutline ("Color from Texture", Float) = 0
		[TCP2KeywordFilter(TCP2_OUTLINE_TEXTURED)] _TexLod ("Texture LOD", Range(0, 10)) = 5
		[Toggle(TCP2_OUTLINE_CONST_SIZE)] _EnableConstSizeOutline ("Constant Size Outline", Float) = 0
		[Toggle(TCP2_ZSMOOTH_ON)] _EnableZSmooth ("Correct Z Artefacts", Float) = 0
		[TCP2KeywordFilter(TCP2_ZSMOOTH_ON)] _ZSmooth ("Z Correction", Range(-3, 3)) = -0.5
		[TCP2KeywordFilter(TCP2_ZSMOOTH_ON)] _Offset1 ("Z Offset 1", Float) = 0
		[TCP2KeywordFilter(TCP2_ZSMOOTH_ON)] _Offset2 ("Z Offset 2", Float) = 0
		[TCP2OutlineNormalsGUI] __outline_gui_dummy__ ("_unused_", Float) = 0
		[TCP2Separator] [HideInInspector] __dummy__ ("unused", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "TCP2_MaterialInspector_SG"
}