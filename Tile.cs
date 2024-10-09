using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformerStarterKit
{
    /// <summary>
    /// Controla la detección de colisiones y el comportamiento de respuesta de una casilla.
    /// </summary>
    enum TileCollision {
        /// <summary>
        /// Una casilla pasable no impide el movimiento al jugador y que pase por ella. A passable tile is one which does not hinder player motion at all.
        /// </summary>
        Passable = 0,

        /// <summary>
        /// Una casilla impasable es aquella sólida, que no permite pasar por ella.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// Una casilla plataforma se comporta como una pasable excepto cuando el jugador esta por encima de ella. Un jugador puede saltar
        /// y atravesar una plataforma por debajo, y moverse a la izquierad y derecha encima de ella, pero no atravesarla hacia abajo.
        /// </summary>
        Platform = 2,
    }

    /// <summary>
    /// Guarda la apariencia y comportamiento de colisión de una casilla.
    /// </summary>
    struct Tile {
        public Texture2D Texture;
        public TileCollision Collision;


        public const int Width = 64;
        public const int Height = 48;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Construye una nueva casilla.
        /// </summary>
        public Tile (Texture2D texture, TileCollision collision) {
            Texture = texture;
            Collision = collision;
        }
    }
}
