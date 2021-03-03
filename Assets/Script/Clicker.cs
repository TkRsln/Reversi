using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicker : MonoBehaviour
{
    private bool isOsAndroid = false;
    private float cam_rot = 0;
    public Transform cam;
    public float sensivity = 5;
    public float rotate_tolerance = 5;
    public float time_tolerance = 1.3f;
    private int[] selected_pos = new int[2];
    private float click_time = 0;
    private float total_rotate = 0;
    private static List<PlayerListener> listeners = new List<PlayerListener>();
    public static void addPlayerListner(PlayerListener listener)
    {
        if (!listeners.Contains(listener)) listeners.Add(listener);
    }
    public static void removePlayerListner(PlayerListener listener)
    {
        if (listeners.Contains(listener)) listeners.Remove(listener);
    }

    private Vector3 mouse_old;
    // Start is called before the first frame update
    void Start()
    {
        isOsAndroid = Application.platform == RuntimePlatform.Android;
        cam = Camera.main.transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)||(Input.touchCount>0&&Input.GetTouch(0).phase==TouchPhase.Began))
        {
            Clicked();
            if (!isOsAndroid) mouse_old = Input.mousePosition;
            cam_rot = 0;
            click_time = Time.time;
            total_rotate = 0;
        }
        else if(Input.GetMouseButton(0)||(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            Vector2 delta= getData();
            cam_rot += delta.x-(cam_rot/9);
            
            rotateCam();
        }else if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {

            //Debug.Log("ended");
            if (total_rotate < rotate_tolerance && (Time.time - click_time) < time_tolerance&&selected_pos[0]!=0) OnClicked();
        }
    }
    private void OnClicked()
    {
        //Debug.Log("clicked!");
        warnListeners(new Vector2Int(selected_pos[0], selected_pos[1]));
    }

    private void rotateCam()
    {
        Quaternion qua = cam.rotation;
        total_rotate +=  cam_rot * sensivity;
        qua.eulerAngles += Vector3.up * cam_rot*sensivity;
        cam.rotation = qua;
    }

    private Vector2 getData()
    {
        if (isOsAndroid)
        {
            return Input.GetTouch(0).deltaPosition;
        }
        else
        {
            Vector3 temp = Input.mousePosition - mouse_old;
            mouse_old = Input.mousePosition;
            return temp;
        }
    }
    void Clicked()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            int z = (int)hit.point.z;
            z += (z + 1) % 2;
            int x = (int)hit.point.x;
            x += (x + 1) % 2;
            selected_pos[0] = x;
            selected_pos[1] = z;

        }else
        {
            selected_pos[0] = 0;
            selected_pos[1] = 0;
        }
    }
    public void warnListeners(Vector2Int pos)
    {
        foreach (PlayerListener p in listeners) p.onClick(pos);
    }
    public interface PlayerListener
    {
        void onClick(Vector2Int pos);
    }
}
