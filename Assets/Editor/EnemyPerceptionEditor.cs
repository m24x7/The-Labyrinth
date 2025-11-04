using UnityEngine;
using UnityEditor;

namespace GDD3400.Labyrinth
{
    /// <summary>
    /// This class provides a custom editor for the EnemyPerception component to visualize perception in the scene view
    /// </summary>
    [CustomEditor(typeof(EnemyPerception))]
    public class EnemyPerceptionEditor : Editor
    {
        /// <summary>
        /// This method is called to draw custom GUI elements in the scene view
        /// </summary>
        private void OnSceneGUI()
        {
            // Get a reference to the EnemyPerception component
            EnemyPerception perception = (EnemyPerception)target;
            
            // Draw the perception radius
            Handles.color = Color.white;
            Handles.DrawWireArc(perception.transform.position, Vector3.up, Vector3.forward, 360, perception.viewRadius);

            // Draw the field of view
            Vector3 viewAngleA = Quaternion.Euler(0, -perception.viewAngle / 2, 0) * perception.transform.forward;
            Handles.DrawLine(perception.transform.position, perception.transform.position + viewAngleA * perception.viewRadius);
            Vector3 viewAngleB = Quaternion.Euler(0, perception.viewAngle / 2, 0) * perception.transform.forward;
            Handles.DrawLine(perception.transform.position, perception.transform.position + viewAngleB * perception.viewRadius);

            // Draw lines to visible targets
            foreach (Transform target in perception.visibleTargets)
            {
                if (target != null)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(perception.transform.position, target.position);
                }
            }
        }
    }
}
