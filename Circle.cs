using System;
using Microsoft.Xna.Framework;

namespace PlatformerStarterKit {
    /// <summary>
    /// Representa un círculo 2D.
    /// </summary>
    struct Circle {
        /// <summary>
        /// Posición central del círculo. 
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Radio del círculo. 
        /// </summary>
        public float Radius;

        /// <summary>
        /// Construye un nuevo círculo.
        /// </summary>
        public Circle (Vector2 position, float radius) {
            Center = position;
            Radius = radius;
        }

        /// <summary>
        ///Determina si un círculo intersecta con un rectangulo. 
        /// </summary>
        /// <returns>True si el círculo y el rectángulo se superposicionan. Falso si no.</returns>
        public bool Intersects (Rectangle rectangle) {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }
    }
}
