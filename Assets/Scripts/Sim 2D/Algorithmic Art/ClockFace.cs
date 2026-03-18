using System.Collections;
using System.Collections.Generic;
//using static System.DateTime;
using UnityEngine;

public class ClockFace : MonoBehaviour
{
    public Vector2 secondHandDiagonal;
    public Vector2 minuteHandDiagonal;
    public Vector2 hourHandDiagonal;

    bool showHands = false;

    private double startTimeSeconds;
    private double startTimeMinutes;
    private double startTimeHours;

    // Start is called before the first frame update
    void Start()
    {
        startTimeSeconds =  System.DateTime.Now.Second + System.DateTime.Now.Millisecond / 1000f;
        startTimeMinutes = System.DateTime.Now.Minute + startTimeSeconds / 60f;
        startTimeHours = System.DateTime.Now.Hour % 12 + startTimeMinutes / 60;
    }

    double secondsTheta(){
        double seconds = Time.realtimeSinceStartupAsDouble % 60 + startTimeSeconds;
        return seconds * 2 * Mathf.PI / 60;
    }

    double minutesTheta(){
        double minutes = Time.realtimeSinceStartupAsDouble % 3600 / 60d + startTimeMinutes + startTimeSeconds / 60d;
        return minutes * 2 * Mathf.PI / 60;
    }

    double hoursTheta(){
        double hours = Time.realtimeSinceStartupAsDouble % 43200 / 3600d + startTimeHours + startTimeMinutes / 60d + startTimeSeconds / 3600d;
        return hours * 2 * Mathf.PI / 12;
    }

    Vector2 handCenter(double theta, Vector2 handSize){
        Vector2 halfSize = handSize / 2f;
        return new Vector2(
            Mathf.Cos((float)theta) * halfSize.x - Mathf.Sin((float)theta) * halfSize.y,
            Mathf.Sin((float)theta) * halfSize.x + Mathf.Cos((float)theta) * halfSize.y
        );
    }

    void OnDrawGizmos(){
        if(!showHands) return;

        Gizmos.color = Color.red;
        Vector2 secondHandCenter = handCenter(secondsTheta(), secondHandDiagonal);
        Gizmos.DrawLine(-secondHandCenter, secondHandCenter);
        Gizmos.color = Color.green;
        Vector2 minuteHandCenter = handCenter(minutesTheta(), minuteHandDiagonal);
        Gizmos.DrawLine(-minuteHandCenter, minuteHandCenter);
        Gizmos.color = Color.blue;
        Vector2 hourHandCenter = handCenter(hoursTheta(), hourHandDiagonal);
        Gizmos.DrawLine(-hourHandCenter, hourHandCenter);
    }
}