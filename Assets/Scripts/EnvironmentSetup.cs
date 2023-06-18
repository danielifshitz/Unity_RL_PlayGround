using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class EnvironmentSetup : MonoBehaviour
{
    public bool IsTraining = false;
    public float StageDifficulty = 1f;

    [SerializeField]
    private float RoomHeight = 5;
    [SerializeField]
    private GameObject LeftWall;
    [SerializeField]
    private GameObject RightWall;
    [SerializeField]
    private GameObject BackWall;
    [SerializeField]
    private GameObject UpperWall;
    [SerializeField]
    private GameObject Brick;
    [SerializeField]
    private GameObject Door;
    [SerializeField]
    private GameObject Windows;
    [SerializeField]
    private GameObject Ground;
    [SerializeField]
    private GameObject OpenSign;
    [SerializeField]
    private GameObject CloseSign;
    [SerializeField]
    private GameObject Boundary;
    [SerializeField]
    private List<DoorType> DoorsType;
    [SerializeField]
    private List<GameObject> EasyObstacleStrocture;
    [SerializeField]
    private List<GameObject> HardObstacleStrocture;

    [HideInInspector]
    public float R = 0;
    [HideInInspector]
    public float L = 0;
    [HideInInspector]
    public float U = 0;
    [HideInInspector]
    public float D = 0;
    [HideInInspector]
    public List<GameObject> Doors = new List<GameObject>();
    [HideInInspector]
    public AIScoreController AIScoreController;
    [HideInInspector]
    public Difficulty StroctureDifficulty = Difficulty.easy;

    public enum Difficulty
    {
        easy,
        hard,
        nightmare
    }

    private List<GameObject> LeftWallBricks = new List<GameObject>();
    private List<GameObject> RightWallBricks = new List<GameObject>();
    private List<GameObject> BackWallBricks = new List<GameObject>();
    private List<GameObject> UpperWallBricks = new List<GameObject>();
    private GameObject CurrentObstacle = null;


    public enum DoorType
    {
        Unlock,
        Locked
    }

    void Awake()
    {
        Brick.GetComponent<Renderer>().enabled = !IsTraining;
        // Windows.GetComponent<Renderer>().enabled = !IsTraining;
        Renderer[] rs = Windows.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
                r.enabled = !IsTraining;

        CallculateCorners();
        SetDoorsTypeOnTraining();
        BuildStructure();
        AddBoundary();
        ChooseStrocture();
    }

    private void Start()
    {
        AIScoreController = AIScoreController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChooseStrocture()
    {
        if (CurrentObstacle != null)
        {
            CurrentObstacle.SetActive(false);
        }

        List<GameObject> ObstacleStrocture;
        if (EasyObstacleStrocture.Count > 0 && HardObstacleStrocture.Count > 0)
        {
            if (StroctureDifficulty == Difficulty.easy)
            {
                ObstacleStrocture = EasyObstacleStrocture;
            }
            else if (StroctureDifficulty == Difficulty.hard)
            {
                ObstacleStrocture = HardObstacleStrocture;
            }
            else
            {
                // TODO
                ObstacleStrocture = new List<GameObject>();
            }
        }
        else
        {
            ObstacleStrocture = EasyObstacleStrocture.Count > 0 ? EasyObstacleStrocture : HardObstacleStrocture;
        }

        int stroctureIndex = Random.Range(0, ObstacleStrocture.Count);
        for (int i = 0; i < ObstacleStrocture.Count; i++)
        {
            if (i == stroctureIndex)
            {
                ObstacleStrocture[i].SetActive(true);
                CurrentObstacle = ObstacleStrocture[i];
            }
            else
            {
                ObstacleStrocture[i].SetActive(false);
            }
        }
    }

    private void SetDoorsTypeOnTraining()
    {
        if (!IsTraining)
        {
            return;
        }
        int OpenDoors = 0;
        foreach (DoorType d in DoorsType)
        {
            if (d == DoorType.Unlock)
            {
                OpenDoors++;
            }
        }

        if (OpenDoors < DoorsType.Count)
        {
            for (int i = 0; i < OpenDoors; i++)
            {
                int IndexOne = Random.Range(0, DoorsType.Count);
                int IndexTwo = Random.Range(0, DoorsType.Count);
                DoorType TempDoor = DoorsType[IndexOne];
                DoorsType[IndexOne] = DoorsType[IndexTwo];
                DoorsType[IndexTwo] = TempDoor;
            }
        }
    }

    private void AddBoundary()
    {
        Boundary.transform.localScale = new Vector3(R - L + 5f, 15f, 0.01f);
        Boundary.transform.position = new Vector3((L + R) / 2, -1f, D + 3f - (15f / 2));
        Boundary.SetActive(true);
    }

    private void CallculateCorners()
    {
        List<Vector3> LocalVertices = new List<Vector3>(Ground.GetComponent<MeshFilter>().mesh.vertices);
        List<Vector3> GlobalVertices = new List<Vector3>();

        foreach (Vector3 point in LocalVertices)
        {
            GlobalVertices.Add(Ground.transform.TransformPoint(point));
        }

        List<int> CornerIDS = new List<int>() { 0, 10, 110, 120 };
        List<Vector3> CornerVertices = new List<Vector3>();

        foreach (int id in CornerIDS)
        {
            CornerVertices.Add(GlobalVertices[id]);
        }

        for (int i = 0; i < 4; i++)
        {
            Vector3 point = CornerVertices[i];
            switch (i)
            {
                case 0:
                    R = point.x;
                    U = point.z;
                    break;
                case 1:
                    L = point.x;
                    break;
                case 2:
                    D = point.z;
                    break;
            }
        }
    }

    private void BuildStructure()
    {
        CreateLeftWall();
        CreateBackWall();
        CreateRightWall();
        CreateUpperWall();
    }

    private void CreateLeftWall()
    {
        if (IsTraining)
        {
            GameObject brick = Instantiate(Brick);
            brick.transform.localScale = new Vector3(Brick.transform.localScale.x, RoomHeight, U - D);
            brick.transform.position = new Vector3(L, brick.transform.localScale.y / 2, (U + D) / 2);
            brick.transform.parent = LeftWall.transform;
            brick.SetActive(true);
            LeftWallBricks.Add(brick);
        }
        else
        {
            for (float j = D; j < U; j++)
            {
                for (float h = 0; h < RoomHeight; h++)
                {
                    GameObject brick = Instantiate(Brick);
                    brick.transform.position = new Vector3(L, h + Brick.transform.localScale.y / 2, j + Brick.transform.localScale.z / 2);
                    brick.transform.parent = LeftWall.transform;
                    brick.SetActive(true);
                    LeftWallBricks.Add(brick);
                }
            }
        }
    }

    private void CreateBackWall()
    {
        if (IsTraining)
        {
            GameObject brick = Instantiate(Brick);
            brick.transform.localScale = new Vector3(Brick.transform.localScale.x, RoomHeight, R - L);
            brick.transform.position = new Vector3((R + L) / 2, brick.transform.localScale.y / 2, U);
            brick.transform.rotation = Quaternion.Euler(0, 90, 0);
            brick.transform.parent = BackWall.transform;
            brick.SetActive(true);
            BackWallBricks.Add(brick);
        }
        else
        {
            for (float j = L, i = 1; j < R; j++, i++)
            {
                for (float h = 0; h < RoomHeight; h++)
                {
                    GameObject brick;
                    if (i != 0 && i % 3 == 0 && h != 0 && (h + 1) % 3 == 0)
                    {
                        brick = Instantiate(Windows);
                        brick.transform.position = new Vector3(j + Windows.transform.localScale.z / 2, h + Windows.transform.localScale.y / 2, U);
                    }

                    else
                    {
                        brick = Instantiate(Brick);
                        brick.transform.position = new Vector3(j + Brick.transform.localScale.z / 2, h + Brick.transform.localScale.y / 2, U);
                    }

                    brick.transform.rotation = Quaternion.Euler(0, 90, 0);
                    brick.transform.parent = BackWall.transform;
                    brick.SetActive(true);
                    BackWallBricks.Add(brick);
                }
            }
        }
    }

    private void CreateRightWall()
    {
        // Debug.Log("IN CreateRightWall");
        int InstalledDoors = 0;
        int DoorsDistance = Mathf.RoundToInt((U - D - DoorsType.Count) / (DoorsType.Count + 1));
        int DistanceCount = DoorsDistance;
        if (IsTraining)
        {
            GameObject brick = Instantiate(Brick);
            brick.transform.localScale = new Vector3(Brick.transform.localScale.x, RoomHeight, U - D);
            brick.transform.position = new Vector3(R, brick.transform.localScale.y / 2, (U + D) / 2);
            brick.transform.parent = RightWall.transform;
            brick.SetActive(true);
            RightWallBricks.Add(brick);

            for (float j = D; j < U; j++)
            {
                if (DistanceCount == 0 && InstalledDoors < DoorsType.Count)
                {
                    GameObject door = Instantiate(Door);
                    GameObject sign;
                    if (DoorsType[InstalledDoors] == DoorType.Unlock)
                    {
                        door.tag = "Open door";
                        sign = Instantiate(OpenSign);
                        sign.transform.position = new Vector3(R, 2 + OpenSign.transform.localScale.y / 2, j + OpenSign.transform.localScale.z / 2);
                    }
                    else
                    {
                        door.tag = "Close door";
                        sign = Instantiate(CloseSign);
                        sign.transform.position = new Vector3(R, 2 + OpenSign.transform.localScale.y / 2, j + OpenSign.transform.localScale.z / 2);
                    }
                    door.transform.position = new Vector3(R, Door.transform.localScale.y / 2, j + Door.transform.localScale.z / 2);
                    door.transform.rotation = Quaternion.Euler(0, 180, 0);
                    Doors.Add(door);

                    sign.transform.rotation = Quaternion.Euler(0, 180, 0);
                    InstalledDoors++;

                    door.transform.parent = RightWall.transform;
                    sign.transform.parent = RightWall.transform;
                    door.SetActive(true);
                    sign.SetActive(true);
                    RightWallBricks.Add(door);
                    RightWallBricks.Add(sign);
                    DistanceCount = DoorsDistance;
                }  
                else
                {
                    DistanceCount--;
                }
            }
        }
        else
        {
            for (float j = D; j < U; j++)
            {
                for (float h = 0; h < RoomHeight; h++)
                {
                    GameObject brick;
                    if (h < 3 && DistanceCount == 0 && InstalledDoors < DoorsType.Count)
                    {
                        if (h == 0)
                        {
                            brick = Instantiate(Door);
                            if (DoorsType[InstalledDoors] == DoorType.Unlock)
                            {
                                brick.tag = "Open door";
                                // BoxCollider boxCollider = brick.GetComponent<BoxCollider>();
                                // boxCollider.isTrigger = true;
                            }
                            else
                            {
                                brick.tag = "Close door";
                            }
                            brick.transform.position = new Vector3(R, Door.transform.localScale.y / 2, j + Door.transform.localScale.z / 2);
                            brick.transform.rotation = Quaternion.Euler(0, 180, 0);
                            Doors.Add(brick);
                        }
                        else if (h == 2)
                        {
                            if (DoorsType[InstalledDoors] == DoorType.Unlock)
                            {
                                brick = Instantiate(OpenSign);
                                brick.transform.position = new Vector3(R, h + OpenSign.transform.localScale.y / 2, j + OpenSign.transform.localScale.z / 2);
                            }

                            else // if ((Doors[InstalledDoors] == DoorType.Locked))
                            {
                                brick = Instantiate(CloseSign);
                                brick.transform.position = new Vector3(R, h + OpenSign.transform.localScale.y / 2, j + OpenSign.transform.localScale.z / 2);
                            }

                            brick.transform.rotation = Quaternion.Euler(0, 180, 0);
                            InstalledDoors++;
                        }
                        else
                            continue;
                    }

                    else
                    {
                        brick = Instantiate(Brick);
                        brick.transform.position = new Vector3(R, h + Brick.transform.localScale.y / 2, j + Brick.transform.localScale.z / 2);
                    }

                    brick.transform.parent = RightWall.transform;
                    brick.SetActive(true);
                    RightWallBricks.Add(brick);
                }

                if (DistanceCount == 0)
                {
                    DistanceCount = DoorsDistance;
                }

                else
                {
                    DistanceCount--;
                }
            }
        }
    }

    private void CreateUpperWall()
    {
        if (IsTraining)
        {
            GameObject brick = Instantiate(Brick);
            brick.transform.localScale = new Vector3(Brick.transform.localScale.x, U - D, R - L);
            brick.transform.position = new Vector3((R + L) / 2, RoomHeight, (U + D) / 2);
            brick.transform.rotation = Quaternion.Euler(0, 90, 90);
            brick.transform.parent = UpperWall.transform;
            brick.SetActive(true);
            UpperWallBricks.Add(brick);
        }
        else
        {
            for (float j = L; j < R; j++)
            {
                for (float h = D; h < U; h++)
                {
                    GameObject brick = Instantiate(Brick);
                    brick.transform.position = new Vector3(j + Brick.transform.localScale.z / 2, RoomHeight, h + Brick.transform.localScale.z / 2);
                    brick.transform.rotation = Quaternion.Euler(0, 90, 90);
                    brick.transform.parent = UpperWall.transform;
                    brick.SetActive(true);
                    UpperWallBricks.Add(brick);
                }
            }
        }
    }

    public void ResetEnvironment()
    {
        if (AIScoreController.GetStageWins(StageDifficulty) > 100)
        {
            StroctureDifficulty = Difficulty.hard;
        }

        // Debug.Log("IN ResetEnvironment");
        int LeftWallLength = RightWallBricks.Count;
        for (int i = LeftWallLength - 1; i >= 0;  i--)
        {
            GameObject brick = RightWallBricks[i];
            RightWallBricks.Remove(brick);
            Destroy(brick);
        }

        for (int i = DoorsType.Count - 1; i >= 0; i--)
        {
            GameObject door = Doors[i];
            Doors.Remove(door);
            Destroy(door);
        }

        SetDoorsTypeOnTraining();
        CreateRightWall();
        ChooseStrocture();
    }
}
