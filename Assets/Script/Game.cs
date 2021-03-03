using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace reversi { 
    public class Game : MonoBehaviour
    {

        public int turn=0;
        private Coroutine rutin;

        public static Game active = null;

        public int team_size = 2;
        public GameObject prefab;
        public GameObject prefab_slot;
        public Material[] team_mats;// 
        public List<GameObject> slot_list = new List<GameObject>();
        public List<PL>[] players;
        private static List<GameListener> listeners = new List<GameListener>();
        public static void addListener(GameListener listener)
        {
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }
        public static void removeListener(GameListener listener)
        {
            if (listeners.Contains(listener)) listeners.Remove(listener);
        }

        private void Awake()
        {
            players= new List<PL>[team_size];
            active = this;
            for (int i = 0; i < team_size; i++) players[i] = new List<PL>(64);
            /*
            addPl(0, new Vector2Int(-1, -1));
            addPl(0, new Vector2Int(-5, -1));
            addPl(0, new Vector2Int(-5, -5));
            addPl(1, new Vector2Int(-3, -1));
            addPl(1, new Vector2Int(-3, -3));
            addPl(1, new Vector2Int(1, -1));
            */
        }

        // Start is called before the first frame update
        void Start()
        {
            
            startGame4();
            //Vector2Int v1 = new Vector2Int(1, -1);
            //Vector2Int v2 = new Vector2Int(-1, 1);

            //StartCoroutine(lateFunc());

            //Debug.Log("Test: " + (v1 + (v2-v1)*2));

        }

        public void startGame2()
        {
            team_size = 2
                ;
            new Player(0);
            new AI(1);

            addPl(0, new Vector2Int(-1, -1));
            addPl(0, new Vector2Int(1, 1));
            addPl(1, new Vector2Int(-1, 1));
            addPl(1, new Vector2Int(1, -1));
            startGame();
        }

        public void startGame4()
        {
            team_size = 4;
            new Player(0);
            new AI(1);
            new AI(2);
            new AI(3);

            addPl(0, new Vector2Int(-1, -1));
            addPl(0, new Vector2Int(-1, -3));
            addPl(1, new Vector2Int(1, 1));
            addPl(1, new Vector2Int(1, 3));
            addPl(2, new Vector2Int(-1, 1));
            addPl(2, new Vector2Int(-3, 1));
            addPl(3, new Vector2Int(1, -1));
            addPl(3, new Vector2Int(3, -1));
            startGame();
        }
        IEnumerator lateFunc()
        {

            addPl(0, new Vector2Int(1, 1));
            addPl(1, new Vector2Int(-1, 1));
            addPl(0, new Vector2Int(-3, 1));
            addPl(1, new Vector2Int(1, -1));
            addPl(0, new Vector2Int(1, -3));
            addPl(1, new Vector2Int(3, 1));
            addPl(0, new Vector2Int(5, 1));
            createPlayableSlot(0);
            yield return new WaitForSeconds(1);
            PL pp = getPlByCoor(new Vector2Int(1, 1));
            PL[][] d=findLines(pp);
            Debug.Log("d: " + d.Length);
            for (int x = 0; x < d.Length; x++)
            {
                Debug.Log(x);
                for(int y = 0; y < d[x].Length; y++)
                {
                    Debug.Log(d[x][y].getX()+" "+d[x][y].getY() );
                }
            }
            //if (d != null) Debug.Log("d: "+d.Length);
            //else Debug.Log("d: Null");

           // removePlayableSlots();
            //createPlayableSlot(1);
            //yield return new WaitForSeconds(5);
            //removePlayableSlots();

        }

        public void setPLToPos(int teamNo,Vector2Int pos)
        {
            PL[][] data = findLines(teamNo, pos);
            addPl(teamNo, pos);

            for(int i = 0; i < data.Length; i++)
            {
                for(int j = 0; j < data[i].Length; j++)
                {
                    data[i][j].team_no = teamNo;
                }
            }


        }
    
        public PL[][] findLines(PL referance)
        {
            List<PL[]> ls = new List<PL[]>();
            for(int i = 0; i < 8; i++)
            {
                int x = (i-1)%8<3 ? 1 : i>4?-1:0;
                int y = (i+1)%8<3?1:((i+2)%8>4?-1:0);//(i-2)%4==0? 0 : ((i)<6 ? -1 : 1);
                int power = 1;
                Vector2Int vec = new Vector2Int(x, y);
                List<PL> line = new List<PL>();
                PL target = getPlByCoor(referance + vec*2*power);
                //Debug.Log((target != null) +" "+(target != referance) + " > " +(referance + vec * 2 * power).x + " | " + (referance + vec * 2 * power).y);
                while(target != null && target!=referance)
                {
                    line.Add(target);
                    //Debug.Log(" > " + i+"  "+x+" "+y);
                    power++;
                    target = getPlByCoor(referance + vec*2*power); 
                    //Debug.Log(">> " + (target != null) + " " + (target != referance) + " > " + (referance + vec * 2 * power).x + " | " + (referance + vec * 2 * power).y);

                }
                if (target != null && target.team_no == referance.team_no && line.Count > 0)
                {
                    ls.Add(line.ToArray());
                    Debug.Log(i + " " + line.Count+" "+line[0].getX() + " " + line[0].getY()+" Direct:"+vec.x+" "+vec.y);
                }

                //if (line.Count > 0) ls.Add(line.ToArray());


            }
            return ls.ToArray();

        }//*/

        public PL[][] findLines(int teamNo,Vector2Int pos)
        {
            List<PL[]> ls = new List<PL[]>();
            for (int i = 0; i < 8; i++)
            {
                int x = i % 4 == 0 ? 0 : (i > 4 ? -1 : 1);
                int y = (i - 2) % 4 == 0 ? 0 : ((i) < 6 ? -1 : 1);
                int power = 1;
                Vector2Int vec = new Vector2Int(x, y);
                List<PL> line = new List<PL>();
                PL target = getPlByCoor(pos + vec * 2 * power);
                //Debug.Log((target != null) +" "+(target != referance) + " > " +(referance + vec * 2 * power).x + " | " + (referance + vec * 2 * power).y);
                while (target != null && target.team_no != teamNo)
                {
                    line.Add(target);
                    //Debug.Log(" > " + i+"  "+x+" "+y);
                    power++;
                    target = getPlByCoor(pos + vec * 2 * power);
                    //Debug.Log(">> " + (target != null) + " " + (target != referance) + " > " + (referance + vec * 2 * power).x + " | " + (referance + vec * 2 * power).y);

                }
                //if (line.Count > 0) ls.Add(line.ToArray());

                if (target != null && target.team_no == teamNo && line.Count > 0) ls.Add(line.ToArray());

            }
            return ls.ToArray();

        }//*/

        public void createPlayableSlot(int teamNo)
        {
            Vector2Int[] arr = findPath(teamNo);
            foreach(Vector2Int v in arr)
            {
                GameObject g = Instantiate(prefab_slot, transform);
                g.transform.position = Vector3.right*v.x+Vector3.forward*v.y;
            
                g.GetComponent<MeshRenderer>().material = Game.active.team_mats[(teamNo * 2)+1];
                //if(isSpaceEmpty(v))
                slot_list.Add(g);
            }
        }
        public void removePlayableSlots()
        {
            for(int i = 0; i < slot_list.Count; i++)
            {
                Destroy(slot_list[i]);
            }
            slot_list.Clear();
        }
        
        public bool addPl(int no,Vector2Int vec)
        {
            if (players[no] != null && isSpaceEmpty(vec)) {
          
                players[no].Add(new PL(vec,no));
                OnCreatePl();
                return true;
            }
            return false;
        }

        public bool isSpaceEmpty(Vector2Int vec)
        {
            for (int i = 0; i < team_size; i++) for (int p = 0; p < players[i].Count; p++) if (players[i][p] == vec) return false;
            return true;
        }
        public PL getPlByCoor(Vector2Int vec)
        {
            for (int i = 0; i < team_size; i++) for (int p = 0; p < players[i].Count; p++) if (players[i][p] == vec) return players[i][p];
            return null;
        }
        public Vector2Int[] findPath(int teamNo)
        {
            List<Vector2Int> l = new List<Vector2Int>();

            for (int i = 0; i<players.Length; i++)
            {
                if (i == teamNo) continue;
                for(int p = 0; p < players[teamNo].Count; p++)
                {
                    for(int t = 0; t < players[i].Count; t++)
                    {
                        Vector2Int vec = players[i][t] - players[teamNo][p];
                        if (3 > vec.magnitude && !l.Contains(players[teamNo][p] + (vec * 2)))
                        {  //3 > vec.magnitude &&
                            if (isSpaceEmpty(players[teamNo][p] + (vec * 2)))l.Add(players[teamNo][p] + (vec * 2));
                            //Debug.Log((players[teamNo][p] + (vec * 2)).x + "    "+ (players[teamNo][p] + (vec * 2)).y);
                        }
                    }
                }
            }
            return l.ToArray();
        }
        private IEnumerator warnListeners()
        {
            foreach (GameListener l in listeners) yield return l.OnTurnChange(turn,findPath(turn));
        }
        public IEnumerator endAITurn()
        {
            turn = (turn + 1) % team_size;
            //createPlayableSlot(turn);
            Debug.Log("T: " + turn);
            yield return warnListeners();
            
        }
        public void endPlayerTurn()
        {
            //turn = (turn + 1) % team_size;
            startGame();
        }
        private void startGame()
        {
            rutin= StartCoroutine(endAITurn());
        }
        public void stopCor()
        {
            StopCoroutine(rutin);
        }
        private void OnCreatePl()
        {

        }
    }

    public class AI : GameListener
    {
        private int team = 0;
        private float wait_max = 2;
        private float wait_min = 3.5f;
        public AI(int team)
        {

            this.team = team;
            Game.addListener(this);
        }



        public IEnumerator OnTurnChange(int teamNo, Vector2Int[] moveable)
        {
            if (teamNo == team) {

                Debug.Log("AI: has turn "+team);
                Game.active.createPlayableSlot(team);
                Game g = Game.active;
                //g.createPlayableSlot(teamNo);
                //int[] t = new int[moveable.Length];
                int count = 0;
                int no = 0;
                for(int i = 0; i < moveable.Length; i++)
                {
                    PL[][] all = g.findLines(teamNo,moveable[i]);
                    int temp = 0;
                    for (int c = 0; c < all.Length; c++) temp += all[c].Length;
                    if (count < temp)
                    {
                        count = temp;
                        no = i;
                    }
                }
                yield return new WaitForSeconds(Random.Range(wait_min, wait_max));
                g.removePlayableSlots();
                Debug.Log("AI: moved to "+ moveable[no].x+" "+ moveable[no].y+" C:"+count);
                g.setPLToPos(team, moveable[no]);
                yield return g.endAITurn();
                

            }
        }
    }

    public class Player : GameListener, Clicker.PlayerListener
    {
        private Vector2Int[] move_temp;
        private int team = 0;
        private bool hasTurn = false;

        public Player(int team)
        {

            this.team = team;
            Game.addListener(this);
            Clicker.addPlayerListner(this);
        }

        public void onClick(Vector2Int pos)
        {
           // Debug.Log("Player: clicked " + pos.x + " " + pos.y);
            if (hasTurn)
            {
                //Debug.Log("Player: hasTurn " + pos.x + " " + pos.y);

                for (int i = 0; i < move_temp.Length; i++)
                {
                    if (move_temp[i] == pos)
                    {
                        Game.active.removePlayableSlots();
                        Game g = Game.active;
                        g.setPLToPos(team,pos);
                        g.endPlayerTurn();
                        hasTurn = false;
                        Debug.Log("Player: Played to "+pos.x+" "+pos.y);
                        return;
                    }
                }
            }
        }

        public IEnumerator OnTurnChange(int teamNo, Vector2Int[] moveable)
        {
            if (moveable.Length == 0)
            {
                yield return new WaitForSeconds(2f);
                Debug.Log("Player: Can't move");
                // TODO NO move
            }else {
                if (teamNo == team)
                {
                    move_temp = moveable;
                    hasTurn = true;
                    yield return new WaitForEndOfFrame();
                    Debug.Log("Player: Has Turn");
                    Game.active.stopCor();
                    Game.active.createPlayableSlot(team);
                    Debug.Log("Player: stop Corutine");
                }
                else
                {
                    hasTurn = false;

                    Debug.Log("Player: has Not Turn");
                }
            }
        }
    }
    public interface GameListener
    {
            IEnumerator OnTurnChange(int teamNo,Vector2Int[] moveable);

    }

    public class PL
    {
        public GameObject pl;
        private int team = 0;
        public int team_no {
            get { return team; }
            set
            {
                OnChange(value);
                team = value;
            }
        }
        private void OnChange(int to)
        { 
            if (to == team) return;
            Game.active.players[team].Remove(this);
            Game.active.players[to].Add(this);
            team = to;
            changeColor();

        }
        private void changeColor()
        {
            if (pl == null) return;
            //Debug.Log((team * 2) +"  " + (pl.transform.GetChild(1) == null));
            pl.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = Game.active.team_mats[(team * 2)];
        }
        public PL(Vector2Int pos, int team)
        {
            team_no = team;
            pl = GameObject.Instantiate(Game.active.prefab, Vector3.right * pos.x + Vector3.forward * pos.y, Game.active.prefab.transform.rotation, Game.active.transform);
            changeColor();
            /*pl.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().materials = new Material[] {
                Game.active.team_mats[2+(team*2)]
                //,Game.active.team_mats[0],
                //Game.active.team_mats[1]
            };//*/

        }
        public int getX()
        {
            return (int)pl.transform.position.x;
        }
        public int getY()
        {
            return (int)pl.transform.position.z;
        }
        public Vector2Int getPosition()
        {
            return new Vector2Int(getX(), getY());
        }

        public static bool operator ==(PL b, Vector2Int c)
        {
            if (b.getX() == c.x && b.getY() == c.y) return true;

            return false;
        }
        public static bool operator !=(PL b, Vector2Int c)
        {
            if (b.getX() == c.x && b.getY() == c.y) return false;
            return true;
        }
        public static bool operator == (PL b, PL c)
        {
            if (object.ReferenceEquals(b, null))
            {
                return object.ReferenceEquals(c, null);
            }
            else if (object.ReferenceEquals(c, null))
            {
                return object.ReferenceEquals(b, null);
            }
            return (b.team_no==c.team_no);
        }
        public static bool operator !=(PL b, PL c)
        {
            if (object.ReferenceEquals(b, null))
            {
                return !object.ReferenceEquals(c, null);
            }
            else if (object.ReferenceEquals(c, null))
            {
                return !object.ReferenceEquals(b, null);
            }
            return b.team_no != c.team_no;
        }
        public static Vector2Int operator +(PL b, PL c)
        {
            return b.getPosition() + c.getPosition();
        }
        public static Vector2Int operator +(PL b,Vector2Int c)
        {
            if (object.ReferenceEquals(b, null))
            {
                return c;
            }
            return b.getPosition() + c;
        }
        public static Vector2Int operator -(PL b, PL c)
        {
            return b.getPosition() - c.getPosition();
        }

    }
}