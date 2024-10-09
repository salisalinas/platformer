using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformerStarterKit {
    /// <summary>
    /// Controla la reproducción de la animación.
    /// </summary>
    struct AnimationPlayer {
        /// <summary>
        /// Pilla la animación que se esta reproduciendo ahora. 
        /// </summary>
        public Animation Animation {
            get { return animation; }
        }
        Animation animation;

        /// <summary>
        /// Coge el index de el frame actual en la animación.
        /// </summary>
        public int FrameIndex {
            get { return frameIndex; }
        }
        int frameIndex;

        /// <summary>
        /// La cantidad de tiempo en segundos que se ha mostrado el frame actual.
        /// </summary>
        private float time;

        /// <summary>
        /// Pilla un origen de textura en el centro abajo de cada frame.Gets a texture origin at the bottom center of each frame.
        /// </summary>
        public Vector2 Origin {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }

        /// <summary>
        /// Empieza o continua el playback de una animación. Begins or continues playback of an animation.
        /// </summary>
        public void PlayAnimation (Animation animation) {
            // Sí esta animación esta corriendo, no la reinicies. 
            if (Animation == animation)
                return;

            //Empieza al nueva animación.
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        /// <summary>
        /// Avanza la posición temporal y dibuja el frame de animación correspondiente. 
        /// </summary>
        public void Draw (GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects) {
            if (Animation == null)
                throw new NotSupportedException("No se está reproduciendo ninguna animación.");

            // Procesa el tiempo pasado. Process passing time.
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > Animation.FrameTime) {
                time -= Animation.FrameTime;

                // Avanza el index del frame; loppeando or Advance the frame index; looping or clamping as appropriate.
                if (Animation.IsLooping) {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                } else {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }

            //Calcula el rectangulo origen del frame actual. 
            Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);

            // Dibuja el frame actual. 
            spriteBatch.Draw(Animation.Texture, position, source, Color.White, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
        }
    }
}
