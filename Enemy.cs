using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformerStarterKit {
    /// <summary>
    /// Dirección a la que esta mirando y su eje X.
    /// </summary>
    enum FaceDirection {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// Un enemigo que nos dificulta progresar.
    /// </summary>
    class Enemy {
        public Level Level {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Posición Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Coge un rectángulo que liga a este enemigo en el espacio del mundo/juego. 
        /// </summary>
        public Rectangle BoundingRectangle {
            get {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;

        /// <summary>
        /// La dirección a la que esta mirand oeste enemigo con su eje X. 
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// Cunato ha estado esperando ete enemigo antes de darse la vuelta. 
        /// </summary>
        private float waitTime;

        /// <summary>
        /// Cuanto tiene que esperar antes de darse la vuelta.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// La velocidad a la que este enemigo se mueve con el eje X.
        /// </summary>

        private const float MoveSpeed = 128.0f;

        /// <summary>
        /// Construye un nuevo enemigo.
        /// </summary>
        public Enemy (Level level, Vector2 position, string spriteSet) {
            this.level = level;
            this.position = position;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Carga un sprite y sonidos de un enemigo en concreto.
        /// </summary>
        public void LoadContent (string spriteSet) {
            // Cargar animaciones.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            sprite.PlayAnimation(idleAnimation);

            // Calcula las medidas del tamaño de la textura.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }


        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public void Update (GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calcula la posición de la casilla basandose en el lado al que estamos caminando.  
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0) {
                // Espera algo de tiempo. 
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f) {
                    // Ahora, date la vuelta.
                    direction = (FaceDirection)(-(int)direction);
                }
            } else {
                // Si vamos a chocar con una pared o llegar a un borde, empieza a esperar. 
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable) {
                    waitTime = MaxWaitTime;
                } else {
                    // Muevete en la dirección actual.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }
        }

        /// <summary>
        /// Dibuja un enemigo animado.
        /// </summary>
        public void Draw (GameTime gameTime, SpriteBatch spriteBatch) {
            // Deja de correr cuando el juego se pause o antes de darte la vuelta. 
            if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                Level.TimeRemaining == TimeSpan.Zero ||
                waitTime > 0) {
                sprite.PlayAnimation(idleAnimation);
            } else {
                sprite.PlayAnimation(runAnimation);
            }


            // Dibuja al enemigo segun su movimiento y su dirección. Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
