#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class ModernGLDevice : IGLDevice
	{
		#region Private OpenGL Entry Points

		// Entry Points

		/* BEGIN GET FUNCTIONS */

		private delegate IntPtr GetString(GLenum pname);
		private GetString INTERNAL_glGetString;
		private string glGetString(GLenum pname)
		{
			unsafe
			{
				return new string((sbyte*) INTERNAL_glGetString(pname));
			}
		}

		private delegate void GetIntegerv(GLenum pname, out int param);
		private GetIntegerv glGetIntegerv;

		/* END GET FUNCTIONS */

		/* BEGIN ENABLE/DISABLE FUNCTIONS */

		private delegate void Enable(GLenum cap);
		private Enable glEnable;

		private delegate void Disable(GLenum cap);
		private Disable glDisable;

		/* END ENABLE/DISABLE FUNCTIONS */

		/* BEGIN VIEWPORT/SCISSOR FUNCTIONS */

		private delegate void G_Viewport(
			int x,
			int y,
			int width,
			int height
		);
		private G_Viewport glViewport;

		private delegate void DepthRange(
			double near_val,
			double far_val
		);
		private DepthRange glDepthRange;

		private delegate void Scissor(
			int x,
			int y,
			int width,
			int height
		);
		private Scissor glScissor;

		/* END VIEWPORT/SCISSOR FUNCTIONS */

		/* BEGIN BLEND STATE FUNCTIONS */

		private delegate void BlendColor(
			float red,
			float green,
			float blue,
			float alpha
		);
		private BlendColor glBlendColor;

		private delegate void BlendFuncSeparate(
			GLenum srcRGB,
			GLenum dstRGB,
			GLenum srcAlpha,
			GLenum dstAlpha
		);
		private BlendFuncSeparate glBlendFuncSeparate;

		private delegate void BlendEquationSeparate(
			GLenum modeRGB,
			GLenum modeAlpha
		);
		private BlendEquationSeparate glBlendEquationSeparate;

		private delegate void ColorMask(
			bool red,
			bool green,
			bool blue,
			bool alpha
		);
		private ColorMask glColorMask;

		private delegate void ColorMaski(
			uint buf,
			bool red,
			bool green,
			bool blue,
			bool alpha
		);
		private ColorMaski glColorMaski;

		private delegate void SampleMaski(uint maskNumber, uint mask);
		private SampleMaski glSampleMaski;

		/* END BLEND STATE FUNCTIONS */

		/* BEGIN DEPTH/STENCIL STATE FUNCTIONS */

		private delegate void DepthMask(bool flag);
		private DepthMask glDepthMask;

		private delegate void DepthFunc(GLenum func);
		private DepthFunc glDepthFunc;

		private delegate void StencilMask(int mask);
		private StencilMask glStencilMask;

		private delegate void StencilFuncSeparate(
			GLenum face,
			GLenum func,
			int reference,
			int mask
		);
		private StencilFuncSeparate glStencilFuncSeparate;

		private delegate void StencilOpSeparate(
			GLenum face,
			GLenum sfail,
			GLenum dpfail,
			GLenum dppass
		);
		private StencilOpSeparate glStencilOpSeparate;

		private delegate void StencilFunc(
			GLenum fail,
			int reference,
			int mask
		);
		private StencilFunc glStencilFunc;

		private delegate void StencilOp(
			GLenum fail,
			GLenum zfail,
			GLenum zpass
		);
		private StencilOp glStencilOp;

		/* END DEPTH/STENCIL STATE FUNCTIONS */

		/* BEGIN RASTERIZER STATE FUNCTIONS */

		private delegate void FrontFace(GLenum mode);
		private FrontFace glFrontFace;

		private delegate void PolygonMode(GLenum face, GLenum mode);
		private PolygonMode glPolygonMode;

		private delegate void PolygonOffset(float factor, float units);
		private PolygonOffset glPolygonOffset;

		/* END RASTERIZER STATE FUNCTIONS */

		/* BEGIN TEXTURE FUNCTIONS */

		private delegate void CreateTextures(
			GLenum target,
			int n,
			out uint textures
		);
		private CreateTextures glCreateTextures;

		private delegate void DeleteTextures(
			int n,
			ref uint textures
		);
		private DeleteTextures glDeleteTextures;

		private delegate void BindTextureUnit(int unit, uint texture);
		private BindTextureUnit glBindTextureUnit;

		private delegate void TextureStorage2D(
			uint texture,
			int levels,
			GLenum internalformat,
			int width,
			int height
		);
		private TextureStorage2D glTextureStorage2D;

		private delegate void TextureSubImage2D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int width,
			int height,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TextureSubImage2D glTextureSubImage2D;

		private delegate void CompressedTextureSubImage2D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int width,
			int height,
			GLenum format,
			int imageSize,
			IntPtr pixels
		);
		private CompressedTextureSubImage2D glCompressedTextureSubImage2D;

		private delegate void TextureStorage3D(
			uint texture,
			int levels,
			GLenum internalFormat,
			int width,
			int height,
			int depth
		);
		private TextureStorage3D glTextureStorage3D;

		private delegate void TextureSubImage3D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int zoffset,
			int width,
			int height,
			int depth,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private TextureSubImage3D glTextureSubImage3D;

		private delegate void CompressedTextureSubImage3D(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int zoffset,
			int width,
			int height,
			int depth,
			GLenum format,
			int imageSize,
			IntPtr pixels
		);
		private CompressedTextureSubImage3D glCompressedTextureSubImage3D;

		private delegate void GetTextureSubImage(
			uint texture,
			int level,
			int xoffset,
			int yoffset,
			int zoffset,
			int width,
			int height,
			int depth,
			GLenum format,
			GLenum type,
			int bufSize,
			IntPtr pixels
		);
		private GetTextureSubImage glGetTextureSubImage;

		private delegate void CreateSamplers(
			int n,
			uint[] samplers
		);
		private CreateSamplers glCreateSamplers;

		private delegate void BindSampler(
			int unit,
			uint sampler
		);
		private BindSampler glBindSampler;

		private delegate void SamplerParameteri(
			uint sampler,
			GLenum pname,
			int param
		);
		private SamplerParameteri glSamplerParameteri;

		private delegate void SamplerParameterf(
			uint sampler,
			GLenum pname,
			float param
		);
		private SamplerParameterf glSamplerParameterf;

		private delegate void PixelStorei(GLenum pname, int param);
		private PixelStorei glPixelStorei;

		/* END TEXTURE FUNCTIONS */

		/* BEGIN BUFFER FUNCTIONS */

		private delegate void CreateBuffers(int n, out uint buffers);
		private CreateBuffers glCreateBuffers;

		private delegate void DeleteBuffers(
			int n,
			ref uint buffers
		);
		private DeleteBuffers glDeleteBuffers;

		private delegate void BindBuffer(GLenum target, uint buffer);
		private BindBuffer glBindBuffer;

		private delegate void NamedBufferData(
			uint buffer,
			IntPtr size,
			IntPtr data,
			GLenum usage
		);
		private NamedBufferData glNamedBufferData;

		private delegate void NamedBufferSubData(
			uint buffer,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private NamedBufferSubData glNamedBufferSubData;

		private delegate void GetNamedBufferSubData(
			uint buffer,
			IntPtr offset,
			IntPtr size,
			IntPtr data
		);
		private GetNamedBufferSubData glGetNamedBufferSubData;

		/* END BUFFER FUNCTIONS */

		/* BEGIN CLEAR FUNCTIONS */

		private delegate void ClearColor(
			float red,
			float green,
			float blue,
			float alpha
		);
		private ClearColor glClearColor;

		private delegate void ClearDepth(double depth);
		private ClearDepth glClearDepth;

		private delegate void ClearStencil(int s);
		private ClearStencil glClearStencil;

		private delegate void G_Clear(GLenum mask);
		private G_Clear glClear;

		/* END CLEAR FUNCTIONS */

		/* BEGIN FRAMEBUFFER FUNCTIONS */

		private delegate void ReadPixels(
			int x,
			int y,
			int width,
			int height,
			GLenum format,
			GLenum type,
			IntPtr pixels
		);
		private ReadPixels glReadPixels;

		private delegate void GenerateTextureMipmap(uint texture);
		private GenerateTextureMipmap glGenerateTextureMipmap;

		private delegate void CreateFramebuffers(
			int n,
			out uint framebuffers
		);
		private CreateFramebuffers glCreateFramebuffers;

		private delegate void DeleteFramebuffers(
			int n,
			ref uint framebuffers
		);
		private DeleteFramebuffers glDeleteFramebuffers;

		private delegate void G_BindFramebuffer(
			GLenum target,
			uint framebuffer
		);
		private G_BindFramebuffer glBindFramebuffer;

		private delegate void NamedFramebufferTexture(
			uint framebuffer,
			GLenum attachment,
			uint texture,
			int level
		);
		private NamedFramebufferTexture glNamedFramebufferTexture;

		private delegate void NamedFramebufferTextureLayer(
			uint framebuffer,
			GLenum attachment,
			uint texture,
			int level,
			int layer
		);
		private NamedFramebufferTextureLayer glNamedFramebufferTextureLayer;

		private delegate void NamedFramebufferRenderbuffer(
			uint framebuffer,
			GLenum attachment,
			GLenum renderbuffertarget,
			uint renderbuffer
		);
		private NamedFramebufferRenderbuffer glNamedFramebufferRenderbuffer;

		private delegate void NamedFramebufferDrawBuffers(
			uint framebuffer,
			int n,
			GLenum[] bufs
		);
		private NamedFramebufferDrawBuffers glNamedFramebufferDrawBuffers;

		private delegate void BlitNamedFramebuffer(
			uint readFramebuffer,
			uint drawFramebuffer,
			int srcX0,
			int srcY0,
			int srcX1,
			int srcY1,
			int dstX0,
			int dstY0,
			int dstX1,
			int dstY1,
			GLenum mask,
			GLenum filter
		);
		private BlitNamedFramebuffer glBlitNamedFramebuffer;

		private delegate void CreateRenderbuffers(
			int n,
			out uint renderbuffers
		);
		private CreateRenderbuffers glCreateRenderbuffers;

		private delegate void DeleteRenderbuffers(
			int n,
			ref uint renderbuffers
		);
		private DeleteRenderbuffers glDeleteRenderbuffers;

		private delegate void NamedRenderbufferStorage(
			uint renderbuffer,
			GLenum internalformat,
			int width,
			int height
		);
		private NamedRenderbufferStorage glNamedRenderbufferStorage;

		private delegate void NamedRenderbufferStorageMultisample(
			uint renderbuffer,
			int samples,
			GLenum internalformat,
			int width,
			int height
		);
		private NamedRenderbufferStorageMultisample glNamedRenderbufferStorageMultisample;

		/* END FRAMEBUFFER FUNCTIONS */

		/* BEGIN VERTEX ATTRIBUTE FUNCTIONS */

		private delegate void VertexAttribPointer(
			int index,
			int size,
			GLenum type,
			bool normalized,
			int stride,
			IntPtr pointer
		);
		private VertexAttribPointer glVertexAttribPointer;

		private delegate void VertexAttribDivisor(
			int index,
			int divisor
		);
		private VertexAttribDivisor glVertexAttribDivisor;

		private delegate void EnableVertexAttribArray(int index);
		private EnableVertexAttribArray glEnableVertexAttribArray;

		private delegate void DisableVertexAttribArray(int index);
		private DisableVertexAttribArray glDisableVertexAttribArray;

		/* END VERTEX ATTRIBUTE FUNCTIONS */

		/* BEGIN DRAWING FUNCTIONS */

		private delegate void DrawRangeElements(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices
		);
		private DrawRangeElements glDrawRangeElements;

		private delegate void DrawElementsInstancedBaseVertex(
			GLenum mode,
			int count,
			GLenum type,
			IntPtr indices,
			int instanceCount,
			int baseVertex
		);
		private DrawElementsInstancedBaseVertex glDrawElementsInstancedBaseVertex;

		private delegate void DrawRangeElementsBaseVertex(
			GLenum mode,
			int start,
			int end,
			int count,
			GLenum type,
			IntPtr indices,
			int baseVertex
		);
		private DrawRangeElementsBaseVertex glDrawRangeElementsBaseVertex;

		private delegate void DrawArrays(
			GLenum mode,
			int first,
			int count
		);
		private DrawArrays glDrawArrays;

		/* END DRAWING FUNCTIONS */

		/* BEGIN QUERY FUNCTIONS */

		private delegate void GenQueries(int n, out uint ids);
		private GenQueries glGenQueries;

		private delegate void DeleteQueries(int n, ref uint ids);
		private DeleteQueries glDeleteQueries;

		private delegate void BeginQuery(GLenum target, uint id);
		private BeginQuery glBeginQuery;

		private delegate void EndQuery(GLenum target);
		private EndQuery glEndQuery;

		private delegate void GetQueryObjectuiv(
			uint id,
			GLenum pname,
			out uint param
		);
		private GetQueryObjectuiv glGetQueryObjectuiv;

		/* END QUERY FUNCTIONS */

		/* BEGIN 3.2 CORE PROFILE FUNCTIONS */

		private delegate IntPtr GetStringi(GLenum pname, uint index);
		private GetStringi INTERNAL_glGetStringi;
		private string glGetStringi(GLenum pname, uint index)
		{
			unsafe
			{
				return new string((sbyte*) INTERNAL_glGetStringi(pname, index));
			}
		}

		private delegate void GenVertexArrays(int n, out uint arrays);
		private GenVertexArrays glGenVertexArrays;

		private delegate void DeleteVertexArrays(int n, ref uint arrays);
		private DeleteVertexArrays glDeleteVertexArrays;

		private delegate void BindVertexArray(uint array);
		private BindVertexArray glBindVertexArray;

		/* END 3.2 CORE PROFILE FUNCTIONS */

#if DEBUG
		/* BEGIN DEBUG OUTPUT FUNCTIONS */

		private delegate void DebugMessageCallback(
			DebugProc callback,
			IntPtr userParam
		);
		private DebugMessageCallback glDebugMessageCallbackARB;

		private delegate void DebugMessageControl(
			GLenum source,
			GLenum type,
			GLenum severity,
			int count,
			IntPtr ids, // const GLuint*
			bool enabled
		);
		private DebugMessageControl glDebugMessageControlARB;

		// ARB_debug_output callback
		private delegate void DebugProc(
			GLenum source,
			GLenum type,
			uint id,
			GLenum severity,
			int length,
			IntPtr message, // const GLchar*
			IntPtr userParam // const GLvoid*
		);
		private DebugProc DebugCall = DebugCallback;
		private static void DebugCallback(
			GLenum source,
			GLenum type,
			uint id,
			GLenum severity,
			int length,
			IntPtr message, // const GLchar*
			IntPtr userParam // const GLvoid*
		) {
			string err = (
				Marshal.PtrToStringAnsi(message) +
				"\n\tSource: " +
				source.ToString() +
				"\n\tType: " +
				type.ToString() +
				"\n\tSeverity: " +
				severity.ToString()
			);
			if (type == GLenum.GL_DEBUG_TYPE_ERROR_ARB)
			{
				FNALoggerEXT.LogError(err);
                if (Debugger.IsAttached)
				    throw new InvalidOperationException("ARB_debug_output found an error: " + err);
			}
            else
			    FNALoggerEXT.LogWarn(err);
		}

		/* END DEBUG OUTPUT FUNCTIONS */

		/* BEGIN STRING MARKER FUNCTIONS */

		private delegate void StringMarkerGREMEDY(int length, byte[] chars);
		private StringMarkerGREMEDY glStringMarkerGREMEDY;

		/* END STRING MARKER FUNCTIONS */
#endif

		private void LoadGLEntryPoints()
		{
			string baseErrorString = "OpenGL 4.5 support is required!";

			/* Basic entry points. If you don't have these, you're screwed. */
			try
			{
				INTERNAL_glGetString = (GetString) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetString"),
					typeof(GetString)
				);
				glGetIntegerv = (GetIntegerv) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetIntegerv"),
					typeof(GetIntegerv)
				);
				glEnable = (Enable) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glEnable"),
					typeof(Enable)
				);
				glDisable = (Disable) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDisable"),
					typeof(Disable)
				);
				glViewport = (G_Viewport) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glViewport"),
					typeof(G_Viewport)
				);
				glDepthRange = (DepthRange) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDepthRange"),
					typeof(DepthRange)
				);
				glScissor = (Scissor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glScissor"),
					typeof(Scissor)
				);
				glBlendColor = (BlendColor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlendColor"),
					typeof(BlendColor)
				);
				glBlendFuncSeparate = (BlendFuncSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlendFuncSeparate"),
					typeof(BlendFuncSeparate)
				);
				glBlendEquationSeparate = (BlendEquationSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlendEquationSeparate"),
					typeof(BlendEquationSeparate)
				);
				glColorMask = (ColorMask) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glColorMask"),
					typeof(ColorMask)
				);
				glColorMaski = (ColorMaski) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glColorMaski"),
					typeof(ColorMaski)
				);
				glDepthMask = (DepthMask) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDepthMask"),
					typeof(DepthMask)
				);
				glDepthFunc = (DepthFunc) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDepthFunc"),
					typeof(DepthFunc)
				);
				glStencilMask = (StencilMask) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilMask"),
					typeof(StencilMask)
				);
				glStencilFuncSeparate = (StencilFuncSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilFuncSeparate"),
					typeof(StencilFuncSeparate)
				);
				glStencilOpSeparate = (StencilOpSeparate) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilOpSeparate"),
					typeof(StencilOpSeparate)
				);
				glStencilFunc = (StencilFunc) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilFunc"),
					typeof(StencilFunc)
				);
				glStencilOp = (StencilOp) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glStencilOp"),
					typeof(StencilOp)
				);
				glFrontFace = (FrontFace) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glFrontFace"),
					typeof(FrontFace)
				);
				glPolygonOffset = (PolygonOffset) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPolygonOffset"),
					typeof(PolygonOffset)
				);
				glPolygonMode = (PolygonMode) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPolygonMode"),
					typeof(PolygonMode)
				);
				glCreateTextures = (CreateTextures) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateTextures"),
					typeof(CreateTextures)
				);
				glDeleteTextures = (DeleteTextures) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteTextures"),
					typeof(DeleteTextures)
				);
				glBindTextureUnit = (BindTextureUnit) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindTextureUnit"),
					typeof(BindTextureUnit)
				);
				glTextureStorage2D = (TextureStorage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureStorage2D"),
					typeof(TextureStorage2D)
				);
				glTextureSubImage2D = (TextureSubImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureSubImage2D"),
					typeof(TextureSubImage2D)
				);
				glCompressedTextureSubImage2D = (CompressedTextureSubImage2D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCompressedTextureSubImage2D"),
					typeof(CompressedTextureSubImage2D)
				);
				glTextureStorage3D = (TextureStorage3D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureStorage3D"),
					typeof(TextureStorage3D)
				);
				glTextureSubImage3D = (TextureSubImage3D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glTextureSubImage3D"),
					typeof(TextureSubImage3D)
				);
				glCompressedTextureSubImage3D = (CompressedTextureSubImage3D) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCompressedTextureSubImage3D"),
					typeof(CompressedTextureSubImage3D)
				);
				glGetTextureSubImage = (GetTextureSubImage) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetTextureSubImage"),
					typeof(GetTextureSubImage)
				);
				glCreateSamplers = (CreateSamplers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateSamplers"),
					typeof(CreateSamplers)
				);
				glBindSampler = (BindSampler) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindSampler"),
					typeof(BindSampler)
				);
				glSamplerParameteri = (SamplerParameteri) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSamplerParameteri"),
					typeof(SamplerParameteri)
				);
				glSamplerParameterf = (SamplerParameterf) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSamplerParameterf"),
					typeof(SamplerParameterf)
				);
				glPixelStorei = (PixelStorei) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glPixelStorei"),
					typeof(PixelStorei)
				);
				glCreateBuffers = (CreateBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateBuffers"),
					typeof(CreateBuffers)
				);
				glDeleteBuffers = (DeleteBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteBuffers"),
					typeof(DeleteBuffers)
				);
				glBindBuffer = (BindBuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindBuffer"),
					typeof(BindBuffer)
				);
				glNamedBufferData = (NamedBufferData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedBufferData"),
					typeof(NamedBufferData)
				);
				glNamedBufferSubData = (NamedBufferSubData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedBufferSubData"),
					typeof(NamedBufferSubData)
				);
				glGetNamedBufferSubData = (GetNamedBufferSubData) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetNamedBufferSubData"),
					typeof(GetNamedBufferSubData)
				);
				glClearColor = (ClearColor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearColor"),
					typeof(ClearColor)
				);
				glClearDepth = (ClearDepth) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearDepth"),
					typeof(ClearDepth)
				);
				glClearStencil = (ClearStencil) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClearStencil"),
					typeof(ClearStencil)
				);
				glClear = (G_Clear) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glClear"),
					typeof(G_Clear)
				);
				glReadPixels = (ReadPixels) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glReadPixels"),
					typeof(ReadPixels)
				);
				glCreateFramebuffers = (CreateFramebuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateFramebuffers"),
					typeof(CreateFramebuffers)
				);
				glDeleteFramebuffers = (DeleteFramebuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteFramebuffers"),
					typeof(DeleteFramebuffers)
				);
				glBindFramebuffer = (G_BindFramebuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBindFramebuffer"),
					typeof(G_BindFramebuffer)
				);
				glNamedFramebufferTexture = (NamedFramebufferTexture) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferTexture"),
					typeof(NamedFramebufferTexture)
				);
				glNamedFramebufferTextureLayer = (NamedFramebufferTextureLayer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferTextureLayer"),
					typeof(NamedFramebufferTextureLayer)
				);
				glNamedFramebufferRenderbuffer = (NamedFramebufferRenderbuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferRenderbuffer"),
					typeof(NamedFramebufferRenderbuffer)
				);
				glNamedFramebufferDrawBuffers = (NamedFramebufferDrawBuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedFramebufferDrawBuffers"),
					typeof(NamedFramebufferDrawBuffers)
				);
				glGenerateTextureMipmap = (GenerateTextureMipmap) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGenerateTextureMipmap"),
					typeof(GenerateTextureMipmap)
				);
				glCreateRenderbuffers = (CreateRenderbuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glCreateRenderbuffers"),
					typeof(CreateRenderbuffers)
				);
				glDeleteRenderbuffers = (DeleteRenderbuffers) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteRenderbuffers"),
					typeof(DeleteRenderbuffers)
				);
				glNamedRenderbufferStorage = (NamedRenderbufferStorage) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedRenderbufferStorage"),
					typeof(NamedRenderbufferStorage)
				);
				glNamedRenderbufferStorageMultisample = (NamedRenderbufferStorageMultisample) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glNamedRenderbufferStorageMultisample"),
					typeof(NamedRenderbufferStorageMultisample)
				);
				glSampleMaski = (SampleMaski) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glSampleMaski"),
					typeof(SampleMaski)
				);
				glBlitNamedFramebuffer = (BlitNamedFramebuffer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBlitNamedFramebuffer"),
					typeof(BlitNamedFramebuffer)
				);
				glVertexAttribPointer = (VertexAttribPointer) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glVertexAttribPointer"),
					typeof(VertexAttribPointer)
				);
				glVertexAttribDivisor = (VertexAttribDivisor) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glVertexAttribDivisor"),
					typeof(VertexAttribDivisor)
				);
				glEnableVertexAttribArray = (EnableVertexAttribArray) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glEnableVertexAttribArray"),
					typeof(EnableVertexAttribArray)
				);
				glDisableVertexAttribArray = (DisableVertexAttribArray) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDisableVertexAttribArray"),
					typeof(DisableVertexAttribArray)
				);
				glDrawArrays = (DrawArrays) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawArrays"),
					typeof(DrawArrays)
				);
				glDrawRangeElements = (DrawRangeElements) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawRangeElements"),
					typeof(DrawRangeElements)
				);
				glDrawRangeElementsBaseVertex = (DrawRangeElementsBaseVertex) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawRangeElementsBaseVertex"),
					typeof(DrawRangeElementsBaseVertex)
				);
				glDrawElementsInstancedBaseVertex = (DrawElementsInstancedBaseVertex) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDrawElementsInstancedBaseVertex"),
					typeof(DrawElementsInstancedBaseVertex)
				);
				glGenQueries = (GenQueries) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGenQueries"),
					typeof(GenQueries)
				);
				glDeleteQueries = (DeleteQueries) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glDeleteQueries"),
					typeof(DeleteQueries)
				);
				glBeginQuery = (BeginQuery) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glBeginQuery"),
					typeof(BeginQuery)
				);
				glEndQuery = (EndQuery) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glEndQuery"),
					typeof(EndQuery)
				);
				glGetQueryObjectuiv = (GetQueryObjectuiv) Marshal.GetDelegateForFunctionPointer(
					SDL.SDL_GL_GetProcAddress("glGetQueryObjectuiv"),
					typeof(GetQueryObjectuiv)
				);
			}
			catch
			{
				throw new NoSuitableGraphicsDeviceException(baseErrorString);
			}

			if (useCoreProfile)
			{
				try
				{
					INTERNAL_glGetStringi = (GetStringi) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glGetStringi"),
						typeof(GetStringi)
					);
					glGenVertexArrays = (GenVertexArrays) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glGenVertexArrays"),
						typeof(GenVertexArrays)
					);
					glDeleteVertexArrays = (DeleteVertexArrays) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glDeleteVertexArrays"),
						typeof(DeleteVertexArrays)
					);
					glBindVertexArray = (BindVertexArray) Marshal.GetDelegateForFunctionPointer(
						SDL.SDL_GL_GetProcAddress("glBindVertexArray"),
						typeof(BindVertexArray)
					);
				}
				catch
				{
					throw new NoSuitableGraphicsDeviceException("OpenGL 3.2 support is required!");
				}
			}

#if DEBUG
			/* ARB_debug_output, for debug contexts */
			IntPtr messageCallback = SDL.SDL_GL_GetProcAddress("glDebugMessageCallbackARB");
			IntPtr messageControl = SDL.SDL_GL_GetProcAddress("glDebugMessageControlARB");
			if (messageCallback == IntPtr.Zero || messageControl == IntPtr.Zero)
			{
				FNALoggerEXT.LogWarn("ARB_debug_output not supported!");
			}
			else
			{
				glDebugMessageCallbackARB = (DebugMessageCallback) Marshal.GetDelegateForFunctionPointer(
					messageCallback,
					typeof(DebugMessageCallback)
				);
				glDebugMessageControlARB = (DebugMessageControl) Marshal.GetDelegateForFunctionPointer(
					messageControl,
					typeof(DebugMessageControl)
				);
				glDebugMessageControlARB(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DONT_CARE,
					GLenum.GL_DONT_CARE,
					0,
					IntPtr.Zero,
					true
				);
				glDebugMessageControlARB(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DEBUG_TYPE_OTHER_ARB,
					GLenum.GL_DEBUG_SEVERITY_LOW_ARB,
					0,
					IntPtr.Zero,
					false
				);
				glDebugMessageControlARB(
					GLenum.GL_DONT_CARE,
					GLenum.GL_DEBUG_TYPE_OTHER_ARB,
					GLenum.GL_DEBUG_SEVERITY_NOTIFICATION_ARB,
					0,
					IntPtr.Zero,
					false
				);
				glDebugMessageCallbackARB(DebugCall, IntPtr.Zero);
			}

			/* GREMEDY_string_marker, for apitrace */
			IntPtr stringMarkerCallback = SDL.SDL_GL_GetProcAddress("glStringMarkerGREMEDY");
			if (stringMarkerCallback == IntPtr.Zero)
			{
				FNALoggerEXT.LogWarn("GREMEDY_string_marker not supported!");
			}
			else
			{
				glStringMarkerGREMEDY = (StringMarkerGREMEDY) Marshal.GetDelegateForFunctionPointer(
					stringMarkerCallback,
					typeof(StringMarkerGREMEDY)
				);
			}
#endif
		}

		#endregion
	}
}
