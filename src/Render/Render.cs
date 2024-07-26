using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.WinForms;

namespace Neoshade;

public static class Renderer {
    public static Vector3[] vertices = new Vector3[4] {
        new Vector3( 1,  1, 0.0f),
        new Vector3( 1, -1, 0.0f),
        new Vector3(-1, -1, 0.0f),
        new Vector3(-1,  1, 0.0f),
    };

    public static Vector2[] texcoords = new Vector2[4] {
        new Vector2(1, -1),
        new Vector2(1, 0),
        new Vector2(0, 0),
        new Vector2(0, -1)
    };

    public static int ProgramId = 0;
    public static int VertexShaderId = 0;
    public static int FragmentShaderId = 0;

    public static string FragmentShader = String.Empty;
    public static string VertexShader = String.Empty;

    public static int VertexArrayObject = 0;
    public static int VBO_Vert = 0;
    public static int VBO_TexCoord = 0;

    public static List<byte> textureData = new List<byte>();
    public static List<byte> previousTextureData = new List<byte>();
    public static bool didUpdateFrame = false;
    public static int width;
    public static int height;
    public static int FrameBufferTexId = 0;
    public static int PreviousFrameBufferId = 0;
    static bool isFirstRender = true;

    public static void Initialize(GLControl control) {
        control.MakeCurrent();

        FragmentShader = File.ReadAllText("shaders/default/default.fs");
        VertexShader = File.ReadAllText("shaders/default/default.vs");

        ProgramId = GL.CreateProgram();
        VertexShaderId = GL.CreateShader(ShaderType.VertexShader);
        FragmentShaderId = GL.CreateShader(ShaderType.FragmentShader);

        GL.ShaderSource(VertexShaderId, VertexShader);
        GL.CompileShader(VertexShaderId);
        GL.ShaderSource(FragmentShaderId, FragmentShader);
        GL.CompileShader(FragmentShaderId);

        GL.AttachShader(ProgramId, VertexShaderId);
        GL.AttachShader(ProgramId, FragmentShaderId);
        GL.LinkProgram(ProgramId);

        GL.DeleteShader(VertexShaderId);
        GL.DeleteShader(FragmentShaderId);

        VertexArrayObject = GL.GenVertexArray();
        VBO_Vert = GL.GenBuffer();
        VBO_TexCoord = GL.GenBuffer();

        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Vert);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vector3.SizeInBytes, vertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_TexCoord);
        GL.BufferData(BufferTarget.ArrayBuffer, texcoords.Length * Vector2.SizeInBytes, texcoords, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);        
        GL.EnableVertexAttribArray(1);

        GL.UseProgram(ProgramId);

        GL.Enable(EnableCap.Texture2D);
        
        FrameBufferTexId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexId);        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureData.ToArray());
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);   

        GL.ActiveTexture(TextureUnit.Texture1);
        PreviousFrameBufferId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, PreviousFrameBufferId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, previousTextureData.ToArray());
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);           


    }
    static Random random = new Random();
    public static void Render(GLControl control) {
        GL.ClearColor(0, 0, 0, 0);
        GL.BindVertexArray(VertexArrayObject);

        if(isFirstRender && textureData.Count > 0 && previousTextureData.Count > 0) {
            GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, textureData.ToArray());
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, PreviousFrameBufferId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, previousTextureData.ToArray());
            isFirstRender = false;
        }

        if(didUpdateFrame) {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexId);
            if(previousTextureData.Count == textureData.Count) {
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, textureData.ToArray());

            }
            else {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, textureData.ToArray());
                
            }

            
        
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, PreviousFrameBufferId);
            if(previousTextureData.Count == textureData.Count) {
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, textureData.ToArray());                
            }
            else {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, previousTextureData.ToArray());
                
            }


            didUpdateFrame = false;        
        }

        GL.Uniform1(GL.GetUniformLocation(ProgramId, "FBTex"), 0);
        GL.Uniform1(GL.GetUniformLocation(ProgramId, "PrevFBTex"), 1);
        GL.Uniform2(GL.GetUniformLocation(ProgramId, "resolution"), new Vector2(width, height));
        GL.Uniform1(GL.GetUniformLocation(ProgramId, "randomizer"), random.NextSingle());
        GL.DrawArrays(BeginMode.TriangleFan, 0, 4);
        control.SwapBuffers();
    }
}