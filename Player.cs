using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PlatformerStarterKit {
    /// <summary>
    /// Nuestro desgraciado personaje.
    /// </summary>
    class Player {
        // Animaciones
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sonidos
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        public Level Level {
            get { return level; }
        }
        Level level;

        public bool IsAlive {
            get { return isAlive; }
        }
        bool isAlive;

        // Físicas
        public Vector2 Position {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constantes para controlar el movimiento horizontal. 
        private const float MoveAcceleration = 14000.0f;
        private const float MaxMoveSpeed = 2000.0f;
        private const float GroundDragFactor = 0.58f;
        private const float AirDragFactor = 0.65f;

        // Constantes para controlar el movimiento vertical.
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -4000.0f;
        private const float GravityAcceleration = 3500.0f;
        private const float MaxFallSpeed = 600.0f;
        private const float JumpControlPower = 0.14f;

        // Configuración de los controles.
        private const float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Detecta si el jugador esta o no en el suelo. 
        /// </summary>
        public bool IsOnGround {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// La tecla pulsada ahora mismo. 
        /// </summary>
        private float movement;

        // Estado en salto.
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Coge un rectangulo que liga al jugador en el espacio del juego. 
        /// </summary>
        public Rectangle BoundingRectangle {
            get {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// Construye un nuevo jugador. 
        /// </summary>
        public Player (Level level, Vector2 position) {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Carga los sprites y los sonidos del jugador.
        /// </summary>
        public void LoadContent () {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);

            // Calcula las medidas dentro del tamaño de la textura.           
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Carga sonidos.           
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        /// <summary>
        /// Resetea al jugador a la vida.
        /// </summary>
        /// <param name="position">En que posición resucitamos.</param>
        public void Reset (Vector2 position) {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Controla el control, ejecuta física y anima el sprite del jugador.
        /// </summary>
        public void Update (GameTime gameTime) {
            GetInput();

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround) {
                if (Math.Abs(Velocity.X) - 0.02f > 0) {
                    sprite.PlayAnimation(runAnimation);
                } else {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            // Limpia el control.
            movement = 0.0f;
            isJumping = false;
        }

        /// <summary>
        /// Pilla el movimiento horizontal del jugador y comandos de salto desde el control.
        /// </summary>
        private void GetInput () {
            // Pilla el estado del control.
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A)) {
                movement = -1.0f;
            } else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D)) {
                movement = 1.0f;
            }

            // Comprueba si el jugador quiere saltar.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics (GameTime gameTime) {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump (float velocityY, GameTime gameTime) {
            // If the player wants to jump
            if (isJumping) {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f) {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime) {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                } else {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            } else {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions () {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y) {
                for (int x = leftTile; x <= rightTile; ++x) {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable) {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero) {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform) {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround) {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            } else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled (Enemy killedBy) {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit () {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw (GameTime gameTime, SpriteBatch spriteBatch) {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
