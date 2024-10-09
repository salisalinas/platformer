using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PlatformerStarterKit
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game {
        // Herramientas para dibujar en pantalla.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Contenido global.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        //Control del nivel del juego.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        private const int TargetFrameRate = 60;
        private const int BackBufferWidth = 1280;
        private const int BackBufferHeight = 720;
        private const Buttons ContinueButton = Buttons.A;


        public PlatformerGame () {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;

            Content.RootDirectory = "Content";

            TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / TargetFrameRate);
        }

        /// <summary>
        /// LoadContent se llama una vez por juego y es donde se carga todo nuestro contenido. 
        /// </summary>
        protected override void LoadContent () {
            // Crea un nuevo SpriteBatch, que se puede usar para dibujar texturas.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Carga fuentes de texto
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Carga las texturas que cubren encima del juego.
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));

            LoadNextLevel();
        }

        /// <summary>
        /// Permite al juego correr su logica como actualizar el mundo, comprobar colisiones,
        /// detectar controles y reproducir audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update (GameTime gameTime) {
            HandleInput();

            level.Update(gameTime);

            base.Update(gameTime);
        }

        private void HandleInput () {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);

            // Sale del juego cuando se pulsa aExit the game when back is pressed.
            if (gamepadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamepadState.IsButtonDown(ContinueButton);

            // Produce la acción adecuada para avanzar el juego y que el jugador siga avanzando.
            if (!wasContinuePressed && continuePressed) {
                if (!level.Player.IsAlive) {
                    level.StartNewLife();
                } else if (level.TimeRemaining == TimeSpan.Zero) {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel () {
            // Busca el camino al siguiente nivel.
            string levelPath;

            //Loopea aquí para intentarlo de nueva en caso de que no podamos encontrar un nivel.
            while (true) {
                // Intenta encontra el siguiente nivel. Son archivos .txt numerados secuencialmente.
                levelPath = String.Format("Content/Levels/{0}.txt", ++levelIndex);
            
                if (File.Exists(levelPath))
                    break;

                // Si no hay nivel 0, algo ha salido mal.
                if (levelIndex == 0)
                    throw new Exception("Error! No se han encontrado niveles.");

                //Cuando no podemos encontrar un nivel, empieza de nuevo de 0.
                levelIndex = -1;
            }

            // Descargar el contenido del nivel actual antes de cargar el siguiente. 
            if (level != null)
                level.Dispose();

            // Carga el nivel.
            level = new Level(Services, levelPath);
        }

        private void ReloadCurrentLevel () {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Dibuja el nivel, tanto el fondo como el primer plano.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw (GameTime gameTime) {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud () {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0) {
                timeColor = Color.Yellow;
            } else {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            // Detremina el mensaje de estado que mostrar.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero) {
                if (level.ReachedExit) {
                    status = winOverlay;
                } else {
                    status = loseOverlay;
                }
            } else if (!level.Player.IsAlive) {
                status = diedOverlay;
            }

            if (status != null) {
                // Dibuja el mensaje de estado.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
        }

        private void DrawShadowedString (SpriteFont font, string value, Vector2 position, Color color) {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
