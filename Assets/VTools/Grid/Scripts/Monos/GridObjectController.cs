using UnityEngine;
using VTools.Utility;

namespace VTools.Grid
{
    public class GridObjectController : MonoBehaviour
    {
        public GridObject GridObject { get; private set; }

        public void Initialize(GridObject gridObject)
        {
            GridObject = gridObject;
        }

        public void ApplyTransform(float localRotation, Vector3 scale)
        {
            transform.localScale = scale;
            transform.localRotation = Quaternion.Euler(0, 0, localRotation);
        }

        public void MoveTo(Vector3 position)
        {
            position.z = 0f; // on bloque sur le plan XY
            transform.position = position;
        }

        public void Rotate(int angle)
        {
            angle = angle.NormalizeAngle();
            transform.localRotation = Quaternion.Euler(0, 0, angle);
            GridObject.Rotate(angle);
        }

        public void AddToGrid(Cell cell, Grid grid, Transform parent)
        {
            GridObject.SetGridData(cell, grid);
            MoveTo(cell.GetCenterPosition(grid.OriginPosition));
        }
    }
}