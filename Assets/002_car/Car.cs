using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Car : MonoBehaviour
{
    [SerializeField] private Transform target;
    public float speed = 2f;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    public float acc = 4f;
    public float accSpeed = 4f;
    public float accAngle = 20f; //触发加减速的角度
    public float steerAngle = 0.25f;
    public float timeScale = 1f;
    public bool useBackupMode = true;

    public List<Vector3> keyPath;
    public int keyPathIdx = 0;
    public bool isBackup = false;
    public BezierLine backupBezier;
    public float backupTime;

    private void CalcPath()
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, 1, path);
        if (path.corners.Length >= 2)
        {
            keyPathIdx = 0;
            keyPath = new List<Vector3>(path.corners);
            transform.position = keyPath[0];
        }
    }

    private void Update()
    {
        var dt = Time.deltaTime * timeScale;

        if (Input.GetMouseButtonDown(0))
        {
            if (keyPath == null || !keyPath.Any())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    target.position = hit.point;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (keyPath == null || !keyPath.Any())
            {
                Debug.Log("make path");
                CalcPath();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (keyPath == null || !keyPath.Any())
            {
                Debug.Log("make path");
                CalcPath();
            }
        }

        if (keyPath == null)
        {
            return;
        }

      
        if (isBackup)
        {
            backupTime += dt * 0.5f;
            var pos = backupBezier.GetPointAtTime(backupTime);
            var moveDir = (pos - transform.position).normalized;
            transform.position = pos;
            transform.forward = -moveDir;
            if (backupTime >= 1)
            {
                isBackup = false;
                acc = 0f;
                speed = minSpeed;
            }

            return;
        }
        
        //碰撞检测
        // if (Physics.Raycast(transform.position, transform.forward, 1f, LayerMask.GetMask("car")))
        // {
        //     return;
        // }


        acc = Mathf.Clamp(acc + accSpeed * dt, 0f, 4f);

        if (keyPathIdx == 0 && keyPath.Count() > 1)
        {
            var currKeyPoint = keyPath[keyPathIdx + 1];
            var hasBlock = NavMesh.Raycast(transform.position, transform.position + transform.forward,
                out NavMeshHit hit0, 1); //前面无法通行需要倒车
            if (hasBlock)
            {
                isBackup = true;
                backupTime = 0;
                var p0 = transform.position;
                var p2 = p0 - transform.forward;
                backupBezier = new BezierLine(p0, p0, p2);
                return;
            }

            if (Vector3.Angle(transform.forward, currKeyPoint - transform.position) > 120 ||
                hasBlock) //角度大于120度需要倒车
            {
                Vector3 midPos = Vector3.Lerp(transform.position, currKeyPoint, 0.35f);
                Vector3 cross = Vector3.Cross((currKeyPoint - transform.position).normalized, Vector3.up);
                Vector3 newPos = Vector3.zero;
                float multi = 2.5f;
                int checkCount = 0;
                bool flag = true;
                while (flag && checkCount < 5)
                {
                    newPos = midPos + cross * multi;
                    if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 0.1f, 1))
                    {
                        flag = false;
                    }
                    else
                    {
                        newPos = midPos - cross * multi;
                        if (NavMesh.SamplePosition(newPos, out NavMeshHit hit2, 0.1f, 1))
                        {
                            flag = false;
                        }
                        else
                        {
                            checkCount++;
                            multi *= 0.8f;
                        }
                    }
                }

                if (useBackupMode)
                {
                    isBackup = true;
                    backupTime = 0;
                    var p0 = transform.position;
                    backupBezier = new BezierLine(p0, Vector3.Lerp(transform.position, currKeyPoint, 0.5f), newPos);
                }
                else
                {
                    keyPath.Insert(1, newPos);
                }

                keyPathIdx++;
                return;
            }
        }

        if (keyPathIdx < keyPath.Count())
        {
            var currKeyPoint = keyPath[keyPathIdx];
            var checkLen = keyPathIdx == keyPath.Count() - 1 ? 0.1f : 2f;
            if (Vector3.Distance(currKeyPoint, transform.position) < checkLen)
            {
                keyPathIdx++;
                int temp = 0;
                if (keyPathIdx + temp < keyPath.Count() - 1)
                {
                    currKeyPoint = keyPath[keyPathIdx];
                    var nextKeyPoint = keyPath[keyPathIdx + 1];
                    var dis = Vector3.Distance(currKeyPoint, nextKeyPoint);
                    if (dis < 1f)
                    {
                        keyPathIdx++;
                    }
                    else if (dis < 2f)
                    {
                        steerAngle = 0.5f;
                    }
                    else
                    {
                        steerAngle = 0.25f;
                    }
                }
            }
            else
            {
                var moveDir =
                    Vector3.RotateTowards(transform.forward, currKeyPoint - transform.position,
                        steerAngle * Mathf.Deg2Rad * timeScale, 0);
                moveDir = moveDir.normalized;
                transform.forward = moveDir;
                if (Vector3.Angle(transform.forward, currKeyPoint - transform.position) < accAngle) //加速
                {
                    speed = Mathf.Clamp(speed + acc * dt, minSpeed, maxSpeed);
                }
                else
                {
                    speed = Mathf.Clamp(speed - acc * dt, minSpeed, maxSpeed);
                }

                transform.position += moveDir * (speed * dt);
            }
        }
        else
        {
            keyPath = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (backupBezier != null)
        {
            backupBezier.drawGizmos();
        }

        if (keyPath != null)
        {
            foreach (var v3 in keyPath)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(v3, 0.6f);
            }
        }
    }
}