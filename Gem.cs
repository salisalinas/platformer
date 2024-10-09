using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PlatformerStarterKit {
    /// <summary>
    /// Lo que el jugador va a coleccionar. 
    /// </summary>
    class Gem {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public const int PointValue = 30;
        public readonly Color Color = Color.Yellow;

        // El objeto se anima desde una posición base junto al eje Y. 
        private Vector2 basePosition;
        private float bounce;

        public Level Level {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Pilla la posiciíon actual de este objeto en el espacio del juego.
        /// </summary>
        public Vector2 Position {
            get {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        /// <summary>
        /// Pilla un círculo que se liga a este objeto en el espacio del juego.
        /// </summary>
        public Circle BoundingCircle {
            get {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

        /// <summary>
        /// Construye un nuevo objeto. 
        /// </summary>
        public Gem (Level level, Vector2 position) {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }

        /// <summary>
        /// Carga la textura y sonidos del objeto. 
        /// </summary>
        public void LoadContent () {
            texture = Level.Content.Load<Texture2D>("Sprites/Gem");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }

        /// <summary>
        ///Esta es la minianimación de flotar en el aire.
        /// </summary>
        public void Update (GameTime gameTime) {
      
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring gems bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        /// <summary>
        /// Se llama cuando este objeto ha sido obtenido por el jugador y borrada del nivel.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this gem. Although currently not used, this parameter would be
        /// useful for creating special powerup gems. For example, a gem could make the player invincible.
        /// </param>
        public void OnCollected (Player collectedBy) {
            collectedSound.Play();
        }

        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        public void Draw (GameTime gameTime, SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
