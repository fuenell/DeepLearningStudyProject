using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;
using LitJson;
using UnityEngine.UI;

public class Generation
{
    public int generationLevel;
    public List<int[]> movementInfos;

    public Generation() { }

    public Generation(int generationLevel, List<int[]> movementInfos)
    {
        this.generationLevel = generationLevel;
        this.movementInfos = movementInfos;
    }
}

public class DeepLearningManager : MonoBehaviour
{
    public const int NUMBER_OF_GENERATION = 50;     // 한 세대의 개체수
    public const int MOVEMENT_INFO_LENGTH = 100;    // 각 Hinge들의 행동 배열의 길이
    public const float moveDelay = 0.1f;            // 행동 딜레이

    public const int bestCnt = 5;                   // 부모 유전자의 개수
    public const int childCnt = 7;                  // 각 부모별 
    public const int changeCnt = 20;

    private readonly string gameDataProjectFilePath = "Generations/Generation";     // json 저장 경로

    public bool isCustom = false;
    public bool isSave = false;

    public int loadGenerationLevel = -1;        // 로드할 세대 레벨

    private Generation currentGeneration;       // 현재 세대

    public GameObject hinge;                    // 경첩 Prefab
    public List<GameObject> hinges;             // 현재 세대의 경첩

    public Text generationLevelText;            // 현재 세대를 UI로 표시할 TEXT

    delegate void Move(int i);                  // 각 경첩의 이동 이벤트를 발생시킬 delegate
    Move move;

    public void Awake()
    {
        // 프레임 고정
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        // 만약 isCustom이 true면 커스텀 배열로 n개의 같은 경첩을 생성한다
        if (isCustom)
        {
            StartCoroutine(CreateCustomHinge(10));
            return;
        }

        // 만약 불러올 세대가 -1이면 완전 랜덤의 1세대를 생성한다
        if (loadGenerationLevel == -1)
        {
            CreateRandomGeneration(1);
        }
        // 불러올 세대가 있으면 그 세대를 불러옴
        else
        {
            LoadGeneration(loadGenerationLevel);
        }

        // 딥러닝 사이클을 시작한다
        StartCoroutine(GenerationCycle());
    }

    // 테스트용 커스텀 경첩 생성
    IEnumerator CreateCustomHinge(int cnt)
    {
        currentGeneration = new Generation
        {
            generationLevel = 0,
            movementInfos = new List<int[]>()
        };

        int[] movementInfo = { 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 0, 1 };

        hinges = new List<GameObject>();
        for (int i = 0; i < cnt; i++)
        {
            currentGeneration.movementInfos.Add(movementInfo);
            hinges.Add(Instantiate(hinge, new Vector3(i * -30, 0, 0), Quaternion.identity));
            HingeControl hingeControl = hinges[i].GetComponent<HingeControl>();
            hingeControl.movementInfo = new int[MOVEMENT_INFO_LENGTH];
            move += new Move(hingeControl.Move);
            for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
            {
                hingeControl.movementInfo[j] = currentGeneration.movementInfos[i][j];
            }
        }

        yield return new WaitForSeconds(3);

        StartMove();
    }

    // 완전 랜덤 세대를 만든다
    private void CreateRandomGeneration(int level)
    {
        currentGeneration = new Generation
        {
            generationLevel = level,
            movementInfos = new List<int[]>()
        };

        for (int i = 0; i < NUMBER_OF_GENERATION; i++)
        {
            int[] movementInfo = new int[MOVEMENT_INFO_LENGTH];
            for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
            {
                movementInfo[j] = Random.Range(0, 2);
            }
            currentGeneration.movementInfos.Add(movementInfo);
        }
    }

    // 세대 사이클
    IEnumerator GenerationCycle()
    {
        while (true)
        {
            // 현재 세대정보로 경첩들을 생성한다
            SettingCurrentGeneration();     
            yield return new WaitForSeconds(1);

            // isSave가 true면 현재 세대를 저장한다
            if (isSave)
                SaveGeneration(currentGeneration);
            yield return new WaitForSeconds(1);

            // 행동배열을 읽기 시작한다
            StartMove();
            yield return new WaitForSeconds(MOVEMENT_INFO_LENGTH * moveDelay + 3);

            // 다음 세대를 생성한다
            CreateNextGeneration(currentGeneration.generationLevel + 1);
            yield return new WaitForSeconds(1);

            // 현재 경첩들을 모두 destroy시킨다
            ClearHinges();
            yield return new WaitForSeconds(1);
        }
    }

    // 다음 세대 생성
    public void CreateNextGeneration(int level)
    {
        int afterChild = bestCnt + (bestCnt * childCnt);      // 부모 + 자식 이후 위치

        int[,] furthestMoves = FindFurthestHinge(bestCnt);    // 최댓값 5개 불러옴

        currentGeneration = new Generation
        {
            generationLevel = level,
            movementInfos = new List<int[]>()
        };

        for (int i = 0; i < NUMBER_OF_GENERATION; i++)
        {
            // 1 ~ 5 : 이전 세대의 최댓값
            if (i < bestCnt)
            {
                int[] movementInfo = new int[MOVEMENT_INFO_LENGTH];
                for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
                {
                    movementInfo[j] = furthestMoves[i, j];
                }
                currentGeneration.movementInfos.Add(movementInfo);
            }
            // 6 ~ 40 : 1의 자식 7개, 2의 자식 7개, 3의 자식 7개
            else if (i < afterChild)
            {
                int[] movementInfo = new int[MOVEMENT_INFO_LENGTH];
                int parentIndex = (i - bestCnt) % bestCnt;
                for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
                {
                    movementInfo[j] = furthestMoves[parentIndex, j];
                }
                currentGeneration.movementInfos.Add(movementInfo);
                // 부모의 유전자에서 changeCnt만큼 랜덤으로 값 수정
                for (int j = 0; j < changeCnt; j++)
                {
                    currentGeneration.movementInfos[i][Random.Range(0, MOVEMENT_INFO_LENGTH)] = Random.Range(0, 2);
                }
            }
            // 41 ~ 50 : 완전 랜덤
            else
            {
                int[] movementInfo = new int[MOVEMENT_INFO_LENGTH];
                for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
                {
                    movementInfo[j] = Random.Range(0, 2);
                }
                currentGeneration.movementInfos.Add(movementInfo);
            }
        }
    }

    // 가장 멀리 간 경첩들을 구한다
    public int[,] FindFurthestHinge(int number)
    {
        // 경첩의 part1과 part2 중 더 멀리 있는 위치를 저장한다
        List<float> posList = new List<float>();
        for (int i = 0; i < hinges.Count; i++)
        {
            float pos1 = hinges[i].transform.Find("Part1").transform.position.z;
            float pos2 = hinges[i].transform.Find("Part2").transform.position.z;
            if (pos1 < pos2)
            {
                pos1 = pos2;
            }
            posList.Add(pos1);
        }

        // 저장된 배열을 복사하고 그 중 하나를 정렬한다
        List<float> tmpPosList = new List<float>(posList);
        posList.Sort();

        // 정렬된 배열과 정렬 안 된 배열을 비교해 가장 멀리 움직인 행동배열 5개를 반환한다
        int[,] furthestHinges = new int[number, MOVEMENT_INFO_LENGTH];
        for (int i = 0; i < number; i++)
        {
            float max = posList[posList.Count - 1 - i];
            int maxIndex = tmpPosList.IndexOf(max);
            if (i == 0)
            {
                Debug.Log(currentGeneration.generationLevel + ":" + max + "/" + maxIndex);
            }
            for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
            {
                furthestHinges[i, j] = hinges[maxIndex].GetComponent<HingeControl>().movementInfo[j];
            }
        }

        return furthestHinges;
    }

    // 경첩 모두 삭제
    private void ClearHinges()
    {
        for (int i = 0; i < hinges.Count; i++)
        {
            Destroy(hinges[i]);
        }
    }

    // 현재 세대 경첩들을 세팅한다
    public void SettingCurrentGeneration()
    {
        generationLevelText.text = currentGeneration.generationLevel + "세대";

        hinges = new List<GameObject>();

        for (int i = 0; i < NUMBER_OF_GENERATION; i++)
        {
            hinges.Add(Instantiate(hinge, new Vector3(i * -30, 0, 0), Quaternion.identity));
            HingeControl hingeControl = hinges[i].GetComponent<HingeControl>();
            hingeControl.movementInfo = new int[MOVEMENT_INFO_LENGTH];
            if (i == 0)
                move = new Move(hingeControl.Move);
            else
                move += new Move(hingeControl.Move);
            for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
            {
                hingeControl.movementInfo[j] = currentGeneration.movementInfos[i][j];
            }
        }
    }

    private int i = 0;
    private float time = 0;
    private bool isMove = false;

    // 이동 시작
    public void StartMove()
    {
        isMove = true;
    }

    private void FixedUpdate()
    {
        if (isMove)
        {
            time += Time.deltaTime;
            if (time >= moveDelay)
            {
                move(i);
                i++;
                if (i >= MOVEMENT_INFO_LENGTH)
                {
                    i = 0;
                    isMove = false;
                }
                time = 0;
            }
        }
    }

    // 세대를 저장한다.
    public void SaveGeneration(Generation generation)
    {
        string jsonData = JsonMapper.ToJson(generation);
        string filePath = Application.dataPath + "/Resources/" + gameDataProjectFilePath + generation.generationLevel + ".json";
        File.WriteAllText(filePath, jsonData);
    }

    // 세대를 불러온다
    public void LoadGeneration(int level)
    {
        TextAsset textAsset = Resources.Load(gameDataProjectFilePath + level) as TextAsset;
        var jsonData = JSON.Parse(textAsset.ToString());
        currentGeneration = new Generation
        {
            generationLevel = jsonData["generationLevel"],
            movementInfos = new List<int[]>()
        };
        for (int i = 0; i < NUMBER_OF_GENERATION; i++)
        {
            int[] movementInfo = new int[MOVEMENT_INFO_LENGTH];
            for (int j = 0; j < MOVEMENT_INFO_LENGTH; j++)
            {
                movementInfo[j] = jsonData["movementInfos"][i][j];
            }
            currentGeneration.movementInfos.Add(movementInfo);
        }
    }
}

