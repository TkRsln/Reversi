using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameV2 : MonoBehaviour
{

    //118
    public static GameV2 active;

    #region Listeners

    public static EventListener count_listener;
    private static List<GameListener> listeners = new List<GameListener>();
    private static List<ConvertAnimation> anims = new List<ConvertAnimation>();
    public void addAnimations(ConvertAnimation ca)
    {
        if (!anims.Contains(ca)) anims.Add(ca);
    }
    public void removeAnimations(ConvertAnimation ca)
    {
        if (anims.Contains(ca)) anims.Remove(ca);
    }
    public static void addListener(GameListener listener)
    {
        if (listeners.Contains(listener)) listeners.Add(listener);
    }
    public static void removeListner(GameListener lis)
    {
        listeners.Remove(lis);
    }
    private void sendCounter(int count)
    {
        if (count_listener != null) count_listener.countFor(count);
    }
    private void sendCounterEnd()
    {
        if (count_listener != null) count_listener.countEnd();
    }
    private void sendCounterBegin()
    {
        if (count_listener != null) count_listener.countBegin(turn);
    }
    private void sendTurnPassed()
    {
        if (count_listener != null) count_listener.TurnPassed(turn);
    }
    private int[] sendTurnDecider(int[][] positions, int turn)
    {
        int[] data = null;
        data=listeners[turn].nextTurn(positions);
        //foreach (GameListener l in listeners)
        //{
        //    data = l.nextTurn(positions, turn);
        //    if (data != null) return data;
        //}
        return data;
    }
    private int sendTurnAnimationDecider(int turn)
    {
        int data = -1;
        foreach (GameListener l in listeners)
        {
            data = l.selectTurnAnimation(turn);
            if (data != -1) return data;
        }
        return -1;
    }
    private int sendSpawnAnimationDecider(int turn)
    {
        int data = -1;
        foreach (GameListener l in listeners)
        {
            data = l.selectSpawnAnimation(turn);
            if (data != -1) return data;
        }
        return -1;
    }

    public interface GameListener
    {
        int[] nextTurn(int[][] positions);
        int selectTurnAnimation(int turn);
        int selectSpawnAnimation(int turn);
    }
    public interface EventListener
    {
        void countFor(int time);
        void countEnd();
        void countBegin(int turn);
        void TurnPassed(int turn);

    }
    #endregion

    #region Game
    [Header("Game")]
    public int team_size = 2;
    public int numberOfAi = 1;
    public int numberOfPlayer = 1;
    public int turn = 0;
    public GameObject prefab;
    public Material[] mats;

    private Coroutine game_turns;
    private const int turn_second = 60;
    private int[] decided_position;

    private void setUpGame()
    {
        setUpAnimation();
        setUpMap();
        createPlayers(numberOfPlayer, numberOfAi);
        startTurn();
    }
    
    public void createPlayers(int player,int AI)
    {
        for (int i = 0; i < player; i++) {
            Player p = new Player();
            listeners.Add(p);
        }
        for (int i = 0; i < AI; i++){
            AI a =new AI(3,player+i);
            listeners.Add(a);
        }
    } 
    public float createPlRuntime(int x,int y,int teamNo,int spawnAnim)
    {
        PL p = createPL(x, y, teamNo);
        if (p == null) return -1;
        if (spawnAnim < 0) return 0;
        return playSpawnAnim(p, spawnAnim);
    }
    public void startTurn()
    {
        Coroutine temp = game_turns;
        game_turns=StartCoroutine(playTurn());
        if (temp != null) StopCoroutine(temp);
    }
    private IEnumerator playTurn()
    {
        int[][] pos = findTeamOption(turn);
        //Debug.Log("G: opsiyonlar bulundu");
        decided_position = sendTurnDecider(pos, turn);
        
        if (decided_position != null) yield return new WaitForSeconds(UnityEngine.Random.Range(2.7f,3.7f));
        else
        {
            //Debug.Log("G: karar verilmedi bekleniyor");
            sendCounterBegin();
            for(int i = 0; i < turn_second*4; i++)
            {
                if (decided_position != null) break;
                else
                {
                    sendCounter(i);
                    yield return new WaitForSeconds(0.25f);
                }
            }
            sendCounterEnd();
        }
        if (decided_position != null)
        {
            Debug.Log("G: Verilen karar oynanıyor");
            int sa = sendSpawnAnimationDecider(turn);
            float spawnAnimDuration = createPlRuntime(decided_position[0], decided_position[1], turn, sa);
            if (spawnAnimDuration < 0) yield return new WaitForSeconds(spawnAnimDuration);
            int turnAnim = sendTurnAnimationDecider(turn);
            PL[][] willTurn = getPlByPosition(decided_position);
            yield return new WaitForSeconds(playTurnAnim(willTurn, map[decided_position[0], decided_position[1]], turn, turnAnim));
            Debug.Log("G: Animasyon Bitti");
        }
        turn = (turn + 1) % team_size;
        startTurn();
    }
    




    #endregion

    #region Animation

    private float playSpawnAnim(PL animToPlay, int no)
    {
        string animName = "s" + no;
        animToPlay.getPlAnim().Play(animName);
        return 0.5f;
    }
    private float playTurnAnim(PL[][] willTurn,PL first,int teamTo,int decidedAnim)
    {
        if (anims.Count == 0||decidedAnim<0) { defaultAnim(willTurn, first, teamTo); return 0; }
        ConvertAnimation c = anims[decidedAnim];
        return c.play(first, willTurn, teamTo);

    }
    private void defaultAnim(PL[][] willTurn, PL first,int teamTo)
    {
        first.teamNo = teamTo;
        foreach (PL[] arr in willTurn)
            foreach (PL p in arr) p.teamNo = teamTo;
        
    }
    private void setUpAnimation()
    {
        SlapAnim sa = new SlapAnim();
        addAnimations(sa);
    }
    #endregion

    #region Map
    [Space]
    [Header("Map")]
    public int map_size = 8;
    private PL[,] map;
    public interface MapListener{
        void onPlCreated(int x, int y);
        void onPlChanged();
    }

    private void setUpMap()
    {
        map = new PL[map_size, map_size];
        createPlayersPosition();
    }

    private void createPlayersPosition()
    {
        switch (team_size)
        {
            case 2:
                createPL(map_size /2-1, map_size / 2 - 2, 0);
                createPL(map_size/2, map_size / 2 - 1, 0);
                createPL(map_size / 2 - 1, map_size / 2 - 1, 1);
                createPL(map_size / 2, map_size / 2 - 2, 1);
                break;
            case 3:
                createPL(map_size / 2 - 1, map_size / 2 - 2, 0);
                createPL(map_size / 2, map_size / 2 - 2, 0);
                createPL(map_size / 2 - 1, map_size / 2 - 1, 0);
                createPL(map_size / 2, map_size / 2 - 1, 0);

                createPL(map_size / 2 - 2, map_size / 2, 1);
                createPL(map_size / 2 - 1, map_size / 2 + 1, 1);
                createPL(map_size / 2 - 3, map_size / 2, 1);
                createPL(map_size / 2 - 2, map_size / 2 + 1, 1);

                createPL(map_size / 2 +1, map_size / 2, 2);
                createPL(map_size / 2, map_size / 2 + 1, 2);
                createPL(map_size / 2 + 1, map_size / 2 + 1, 2);
                createPL(map_size / 2 + 2, map_size / 2, 2);
                break;

            case 4:


                break;
        }
    }
    private PL createPL(int x,int y,int teamNo)
    {
        x = Mathf.Clamp(x, 0, map_size);
        y = Mathf.Clamp(y, 0, map_size);

        if (getPlType(x, y) != -1) return null;
        map[x,y] = new PL(x, y, teamNo);
        return map[x, y];
        // todo bitmedi
    }
    private PL[][] getPlByPosition(int[] pos)
    {
        /*
         * Pos arrayin 3. değerinden sonraki değerler {x,y} şeklinde en yakın, aynı takıma ait PL konumunu söyler
         * Bu Konumdan yola çıkarak, aradaki Pl'ler bulunur.
         * 
         *  Örn:   fx,fy             pos[3],pos[4]
         *          *------------------*                arada kalan Pl'leri bulur
         *          
         */
        int to = (pos.Length - 3) / 2;
        int fx = pos[0];
        int fy = pos[1];
        List<PL[]> l = new List<PL[]>();
        for (int i = 0; i < to; i++)
        {
            int tx = pos[3 + i*2];
            int ty = pos[3 + i*2+1];
            int dx = tx - fx;
            int dy = ty - fy;
            int ax = dx==0?0:(dx) / Mathf.Abs(dx);
            int ay = dy == 0 ? 0 : (dy) / Mathf.Abs(dy);
            int countTo = Mathf.Abs((dx == 0 ? dy : dx))-1;
            //Debug.Log("PLPOS: " + countTo);
            PL[] arry = new PL[countTo];
            for(int c = 1; c <= countTo; c++)
            {
                //Debug.Log("Found: " + "Cou: " + c + " ay:" + ay + " ax:" + ax + " P:" + map[fx + (ax * c), fy + (ay * c)].getGamePosition());
                arry[c-1] = map[fx+(ax*c), fy+ (ay * c)];
            }
            l.Add(arry);            
        }
        return l.ToArray();
    }
    public Vector2Int createDirection(int x,int y)
    {
        return Vector2Int.right * x + Vector2Int.up * y;
    }
    public int[][] canJumpFrom(int x,int y,int teamNo) // Tüm yönleri kontrol ederek şartları sağlayan lokasyonları hafızaya alır
    {
        List<int[]> all = new List<int[]>();
        for (int i = 0; i < 8; i++)
        {
            int dx = (i - 1) % 8 < 3 ? 1 : (i > 4 ? -1 : 0);
            int dy = (i + 1) % 8 < 3 ? 1 : ((i + 2) % 8 > 4 ? -1 : 0);//(i-2)%4==0? 0 : ((i)<6 ? -1 : 1);
            int[] jump = canJumpTo(x, y, teamNo, createDirection(dx, dy));
            if (jump != null)
            {
                if (!all.Contains(jump)) all.Add(jump);
                //else all.Find(x => { return x[0] == jump[0] && x[1] == jump[1];})[2] += jump[2];

            }
        }
        //counts.OrderBy(lc => lc.Count)
        //return all.OrderBy(lc => Vector2Int.Distance(new Vector2Int(lc[0],lc[1]),new Vector2Int(x,y))).ToArray();
        return all.ToArray();
    } 
    public int[][] findTeamOption(int teamNo)
    {
        List<int[]> all = new List<int[]>();
        for(int p = 0; p < map_size*map_size; p++)
        {
            int x = p % map_size;
            int y = p / map_size;
            if (getPlType(x, y) != teamNo) continue;
            for (int i = 0; i < 8; i++)
            {
                int dx = (i+7) % 8 < 3 ? 1 : (i > 4 ? -1 : 0);
                int dy = (i + 1) % 8 < 3 ? 1 : ((i + 2) % 8 > 4 ? -1 : 0);//(i-2)%4==0? 0 : ((i)<6 ? -1 : 1);
                //Debug.Log("P: " + x + " " + y + " i:" + i + " xy:"+dx+" "+dy+" t:" + getPlType(x, y));
                int[] jump = canJumpTo(x, y, teamNo, createDirection(dx, dy));
                if (jump != null)
                {
                    int indx = isContains(all, jump);
                    if (indx == -1) all.Add(jump);
                    else
                    {
                        int[] temp = all[indx];
                        all[indx] = new int[temp.Length + 2];
                        for (int j = 0; j < temp.Length; j++) all[indx][j] = temp[j];
                        all[indx][2] += jump[2];
                        all[indx][temp.Length] = jump[3];
                        all[indx][temp.Length+1] = jump[4];
                    }
                }
            }
        }

        return all.OrderBy(c =>c[2]).ToArray();    
    }
    private int isContains(List<int[]> l, int[] coordinate) {
        for(int i=0;i<l.Count;i++)
        {
            int[] a = l[i];
            if (a[0] == coordinate[0] && a[1] == coordinate[1]) return i;
        }
        return -1;
    }

    public int[] canJumpTo(int x,int y,int teamNo,Vector2Int direction) // Verilen yöndeki ilk şartı(bitişiğinde düşman olması) kontrol eder
                                                                        // şart sağlanırsa boşluk bulmaya çalışır - findSpace();
    {
        if (x + direction.x >= map_size || y + direction.y >= map_size) return null;
        if (x + direction.x < 0 || y + direction.y < 0) return null;
        int type = getPlType(x+direction.x, y+direction.y);
        //Debug.Log(x + " " + y + " " + type+" "+direction.ToString()+" M:"+ (x + direction.x)+" "+ (y + direction.y));
        if (type != -1 && type != teamNo) {
            //Debug.Log("CJ: found enemy...");
            return findSpace(x+direction.x, y+direction.y, teamNo,direction);
        }else return null;
    }
    private int[] findSpace(int x,int y,int teamNo, Vector2Int direction)
    {
        //Debug.Log("FS Start: " + (x ) + " " + (y )+" "+direction);
        return findSpaceMain(x, y, teamNo, direction, 1);
    }
    private int[] findSpaceMain(int x, int y, int teamNo, Vector2Int direction, int count)
    {
        //count++;
        int dx = x + direction.x;
        int dy = y + direction.y;
        if (dx >= map_size || dy >= map_size) return null;
        if (dx < 0 || dy < 0) return null;
        int type = getPlType(dx, dy);
        if (type != -1)
        {
            if (type != teamNo) return findSpaceMain(x + direction.x, y + direction.y, teamNo, direction, count+1);
            else return null;
        }
        else
        {

           // Debug.Log("FS: "+(x + direction.x)+" "+(y + direction.y));
            return new int[] { x + direction.x, y + direction.y, count,x-direction.x*count, y - direction.y * count };
        }
    }

    //-------------------------
    

    private int getPlType(int x,int y)
    {
        //if(x==2&&y==3)Debug.Log("GT: " + x + " " + y+" "+ map[x, y].teamNo);
        if (map[x, y] == null) return -1;
        return map[x,y].teamNo;
    }

    #endregion

    private void Awake()
    {
        active = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        setUpGame();
        /*
        createPL(3, 3, 0);
        createPL(2, 3, 1);
        createPL(1, 2, 1);
        createPL(1, 1, 0);

        int[][] pos = findTeamOption(0);

        for(int i = 0; i < pos.Length; i++)
        {
            Debug.Log(i+" s:"+pos[i][0]+" "+ pos[i][1]+" "+ pos[i][2] + " " + pos[i][3] + " " + pos[i][4] + " " + pos[i][5] + " " + pos[i][6]);
        }
        */
    }

}

public class AI : GameV2.GameListener
{
    private int level = 2;
    private int team = 0;
    //private float wait_min = 2.7f;
    //private float wait_max = 3.5f;

    //public float waitForMax() { return wait_max; }
    //public float waitForMin() { return wait_min; }

    public AI(int dificulty,int team)
    {
        level = Mathf.Clamp(dificulty, 0, 2);
        this.team = team;

    }

    //public AI()

    public int[] nextTurn(int[][] positions)
    {

        //Debug.Log("AI: return 0;");
        return positions[0];

    }


    public int selectSpawnAnimation(int turn)
    {
        return -1;
    }

    public int selectTurnAnimation(int turn)
    {
        return -1;
    }
}

public class Player : GameV2.GameListener
{
    public int[] nextTurn(int[][] positions)
    {

        //Debug.Log("P: return 0;");
        return positions[positions.Length-1];
    }

    public int selectSpawnAnimation(int turn)
    {
        return -1;
    }

    public int selectTurnAnimation(int turn)
    {
        return 0;
    }
}

public class SlapAnim : ConvertAnimation
{

    private string firstAnim="a0";
    private float firstWait=0.5f;

    private string otherAnim="r0";
    private float otherWait=0.3f;
    private float otherChange=0.3f;
    private Coroutine rutine;

    

    public override float play(PL first, PL[][] others, int teamToChange)
    {
        float total = 0.5f;
        for (int i = 0; i < others.Length; i++) total+=others[i].Length*otherWait;
        total+= (firstWait * others.Length);
        rutine = GameV2.active.StartCoroutine(startAnim(first, others,teamToChange));
        return total;
    }
    private IEnumerator startAnim(PL first, PL[][] others, int teamToChange)
    {
        first.getPlAnim().Play("s0");
        yield return new WaitForSeconds(0.5f);
        int count = 0;
        for (int o = 0; o < others.Length; o++) count+=others[o].Length;
        Debug.Log("Anim: t:"+teamToChange+" c:" + others.Length + " " + count);
        //yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < others.Length; i++)
        {

            first.lookAt(others[i][0].getTransform());
            first.getPlAnim().Play(firstAnim);
            if (firstWait != 0) yield return new WaitForSeconds(firstWait);
            for (int j = 0; j < others[i].Length; j++)
            {
                Debug.Log("Loop: "+i+" "+j+" p:" + others[i][j].getGamePosition());
                others[i][j].getPlAnim().Play(otherAnim);
                yield return new WaitForSeconds(otherWait/2);
                others[i][j].teamNo = teamToChange;
                yield return new WaitForSeconds(otherWait / 2);

                //others[i][j].getPlAnim().enabled = false;
            }

        }
        

    }
}

public class ConvertAnimation
{
    public virtual float play(PL first, PL[][] others, int teamToChange) { return 0; }

    /*
    private string firstAnim;
    private float firstWait;

    private string otherAnim;
    private float otherWait;
    private float otherChange;

    public ConvertAnimation(string[] firstAnim,float[] waitFirst,bool playFirstsAll,string animOther,float waitOther,float changeTeamAfter)
    {
        this.firstAnim = firstAnim;
        firstWait = waitFirst;
        otherAnim = animOther;
        otherWait = waitOther;
        otherChange = changeTeamAfter;
    }

    */

}

public class PL
{
    public int teamNo
    {
        get { return team; }
        set { if (value != team) { onTeamChange(value); team = value; } }
    }

    private GameObject obj = null;
    private int team=-1;
    private int x = 0;
    private int y = 0;

    private float x_space = 2;
    private float y_space = 2;

    public Vector2Int getGamePosition()
    {
        return new Vector2Int(x, y);
    }
    public Vector3 getWorldPosition()
    {
        return xyToWorldPosition(x, y);
    }

    public Transform getTransform()
    {
        return obj.transform;
    }
    public Animator getPlAnim()
    {
        return obj.GetComponent<Animator>();
    }
    public PL(int x,int y,int teamNo)
    {
        obj = GameObject.Instantiate(GameV2.active.prefab,GameV2.active.transform);
        obj.transform.position = xyToWorldPosition(x, y);
        this.x = x;
        this.y = y;
        this.teamNo = teamNo;
    }
    private void onTeamChange(int to)
    {
        obj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = GameV2.active.mats[to * 2];
    }
    public void lookAt(Transform target)
    {
        obj.transform.LookAt(target);
    }

    public Vector3 xyToWorldPosition(int x,int y)
    {
        return new Vector3((-GameV2.active.map_size / 2 + x) * x_space + x_space / 2, 0,( - GameV2.active.map_size / 2 + y) * y_space+y_space/2); 
    }

}