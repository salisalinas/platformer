using System;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformerStarterKit {
    /// <summary>
    /// Representa una textura animada.
    /// </summary>
    /// <remarks>
    /// Esta clase asume que cada frame de animacion es tan amplio
    /// como cuan alta es cada animacion. El número de frames en la animación se sacan de esto.
    /// </remarks>
    class Animation {
        /// <summary>
        /// Todos los frames en la animación se ordenan horizontalmente. 
        /// </summary>
        public Texture2D Texture {
            get { return texture; }
        }
        Texture2D texture;

        /// <summary>
        /// Duración de cada frame. 
        /// </summary>
        public float FrameTime {
            get { return frameTime; }
        }
        float frameTime;

        /// <summary>
        /// Cuando se llega al final de la animación, debería repetirse desde el principio?
        /// </summary>
        public bool IsLooping {
            get { return isLooping; }
        }
        bool isLooping;

        /// <summary>
        /// Pilla el número de frames en la animación.
        /// </summary>
        public int FrameCount {
            get { return Texture.Width / FrameWidth; }
        }

        /// <summary>
        /// Coge el ancho de un frame en la animación.
        /// </summary>
        public int FrameWidth {
            // Coge frames cuadrados.
            get { return Texture.Height; }
        }

        /// <summary>
        /// Coge la altura de un frame en la animación.
        /// </summary>
        public int FrameHeight {
            get { return Texture.Height; }
        }

        /// <summary>
        /// Construye una nueva animación.
        /// </summary>        
        public Animation (Texture2D texture, float frameTime, bool isLooping) {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }
    }
}
