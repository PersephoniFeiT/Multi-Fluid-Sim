using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: HARDCODE 3 OBSTACLES IN COMPUTE SINCE PARAMETERIZING THEM AS BUFFERS IS SUPER LAGGY TO UPDATE EVERY FRAME!!!!
public class DynamicObstacle : MonoBehaviour
{
    public Vector2 size; //the diagonal vector of the rectangle or x = length, y = width
    public Vector2 centre;
    public float rotation; //in radians clockwise

    public void OnDrawGizmos(){
        Vector2 halfSize = size / 2f;
        Vector2 right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
        Vector2 up = new Vector2(-Mathf.Sin(rotation), Mathf.Cos(rotation));
        Vector2 corner1 = centre + right * halfSize.x + up * halfSize.y;
        Vector2 corner2 = centre - right * halfSize.x + up * halfSize.y;
        Vector2 corner3 = centre - right * halfSize.x - up * halfSize.y;
        Vector2 corner4 = centre + right * halfSize.x - up * halfSize.y;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(corner1, corner2);
        Gizmos.DrawLine(corner2, corner3);
        Gizmos.DrawLine(corner3, corner4);
        Gizmos.DrawLine(corner4, corner1);
    }
}
