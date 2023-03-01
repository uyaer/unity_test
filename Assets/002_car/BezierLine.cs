using UnityEngine;

public class BezierLine 
{
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public int fragment = 20;

    public BezierLine(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
    }

    public void drawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(p0,0.5f);
        Gizmos.DrawWireSphere(p1,0.5f);
        Gizmos.DrawWireSphere(p2,0.5f);
        Vector3 begin = p0;
        int i = 0;
        do
        {
            Vector3 pos = GetPointAtTime((i + 1) / (float)fragment);
            Gizmos.DrawLine(begin, pos);
            begin = pos;
            i++;
        } while (i < fragment);
#endif
    }

    public Vector3 GetPointAtTime(float time)
    {
        return (1 - time) * (1 - time) * p0 + 2 * time * (1 - time) * p1 + time * time * p2;
    }
}