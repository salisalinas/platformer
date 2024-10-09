using System;
using Microsoft.Xna.Framework;

namespace PlatformerStarterKit {
    /// <summary>
    /// Una serie de méotods para ayudarnos a trabajar con rectángulos. 
    /// </summary>
    public static class RectangleExtensions {
        /// <summary>
        /// Calcula la profundidad indicada de la intersección entre dos rectángulos. 
        /// </summary>
        /// <returns>
        /// La cantidad de superposición entre dos rectángulos que se cruzan. Estos valores de profundidad
        /// pueden ser negativos dependiendo de en que lados se cruzan los rectángulos. Esto permite a los llamadores determinar
        /// depth values can be negative depending on which wides the rectangles
        /// la dirección correcta para empujar objetos para sus colisiones. This allows callers to determine the correct direction
        /// to push objects in order to resolve collisions.
        /// Si los rectángulos no se están chocando, Vector2.Zero se devuelve.
        /// </returns>
        public static Vector2 GetIntersectionDepth (this Rectangle rectA, Rectangle rectB) {
            //Calculate half sizes.
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calcula los centros.
            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // If we are not intersecting at all, return (0, 0).
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // Calculate and return intersection depths.
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }

        /// <summary>
        /// Pilla la posición del centro del borde inferior de rectángulo.
        /// </summary>
        public static Vector2 GetBottomCenter (this Rectangle rect) {
            return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
        }
    }
}
