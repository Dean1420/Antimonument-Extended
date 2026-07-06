using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StatueSpawnControllerScene1 : MonoBehaviour
{
    public static StatueSpawnControllerScene1 Singleton;
    //[SerializeField] TexturePainter TexPaint;
    //[SerializeField] SoundManager soundManager;
    //public PassthroughManager PassthroughManager;


    public ReflectionProbe Probe;
    string TagPaint = "Paintable";
    string TagInter = "Interactable";

    public List<StatueValues> AvailableStatues = new List<StatueValues>();
    public Socket SocketPrefab;
    public Transform SocketParentTransform;
    public HardReset AddOnController;
    public SliceObject sliceController;

    private StatueValues _currentStatue;
    private List<Statue> _statueInstances = new List<Statue>();

    private List<Socket> _sockets = new List<Socket>();
    // private Statue _statueInstance;

    void Awake()
    {
        Singleton = this;
    }
    void Start()
    {
        //RenderSettings.skybox = TexPaint.SkyBoxHub;
        InitSockets();
    }

    private void InitSockets()
    {
        _statueInstances = new List<Statue>();
        int index = 0;
        foreach (var statue in AvailableStatues)
        {
            var socket = Instantiate(SocketPrefab, SocketParentTransform);
            socket.transform.localPosition = Vector3.zero;
            socket.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            socket.transform.localPosition = Vector3.right * index;
            socket.Init(statue);
            _sockets.Add(socket);

            var lifeSizeStatue = Instantiate(statue.LifeSizeStatuePrefab);
            lifeSizeStatue.transform.localPosition = statue.StatueSpawnPoint;
            lifeSizeStatue.transform.localRotation = Quaternion.Euler(statue.StatueSpawnRotation);
            lifeSizeStatue.transform.localScale *= statue.ScaleFactor;
            lifeSizeStatue.StatueValues = statue;
            lifeSizeStatue.gameObject.SetActive(false);
            _statueInstances.Add(lifeSizeStatue);

            if (index == 0)
                index++;
            else if (index < 0)
            {
                index = (index - 1) * -1;
            }
            else
            {
                index *= -1;
            }
        }

    }
    void OnTriggerEnter(Collider col)
    {
        var statue = col.GetComponent<Statue>();
        Debug.Log("Statue entered: " + (statue) + " from " + col.gameObject.name);
        if (statue != null)
        {
            if (statue.StatueValues != _currentStatue)
            {
                _currentStatue = statue.StatueValues;
                SetSockelActive(_currentStatue);
            }

        }
    }


    public void DestroyStatue(string TagStatue)
    {
        GameObject[] DestroyStat = GameObject.FindGameObjectsWithTag(TagStatue);
        for (int i = 0; i < DestroyStat.Length; i++)
        {
            Destroy(DestroyStat[i]);
        }
    }


    public void DestroyStatueReal(string TagStatue)
    {
        GameObject[] DestroyStat = GameObject.FindGameObjectsWithTag(TagStatue);
        for (int i = 0; i < DestroyStat.Length; i++)
        {
            if (DestroyStat[i].name == "SlicedUp" || DestroyStat[i].name == "SlicedLow")
            {
                Destroy(DestroyStat[i]);
            }
        }
    }


    // public void SockelLoewe(bool Active)
    // {
    //     DestroyStatueReal(TagPaint);
    //     DestroyStatueReal(TagInter);
    //
    //    // DestroyStatue(TagLoewe);
    //    // DestroyStatue(TagSchnecke);
    //    // DestroyStatue(TagJueng);
    //     loeweActive = Active;
    //     schneckeActive = false;
    //     juenglingActive = false;
    //     TexPaint.restoreMaterial();
    //     TexPaint.LöweStatue.SetActive(Active);
    //     TexPaint.SchneckeStatue.SetActive(false);
    //     TexPaint.JuenglingStatue.SetActive(false);
    //
    //     TexPaint.statue = TexPaint.LöweStatue;
    //     TexPaint.tex = TexPaint.LöweTex;
    //     TexPaint.tex2 = TexPaint.LöweTexTemp;
    //
    //     RenderSettings.skybox = TexPaint.SkyBoxLöwe;
    //     DynamicGI.UpdateEnvironment();
    //     Probe.RenderProbe();
    //
    //     //statue.GetComponent<MeshRenderer>().material = baseMaterial;
    //     TexPaint.statue.GetComponent<MeshRenderer>().material.mainTexture = TexPaint.canvasTexture;
    //     //rendTex.GetComponent<MeshRenderer>().material = baseMaterial;
    //     TexPaint.rendTex.GetComponent<MeshRenderer>().material.mainTexture = TexPaint.tex;
    // }
    // public void SockelSchnecke(bool Active)
    // {
    //     DestroyStatueReal(TagPaint);
    //     DestroyStatueReal(TagInter);
    //
    //     //DestroyStatue(TagLoewe);
    //    // DestroyStatue(TagSchnecke);
    //     //DestroyStatue(TagJueng);
    //     loeweActive = false;
    //     schneckeActive = Active;
    //     juenglingActive = false;
    //     TexPaint.restoreMaterial();
    //     TexPaint.LöweStatue.SetActive(false);
    //     TexPaint.SchneckeStatue.SetActive(Active);
    //     TexPaint.JuenglingStatue.SetActive(false);
    //
    //     TexPaint.statue = TexPaint.SchneckeStatue;
    //     TexPaint.tex = TexPaint.SchneckeTex;
    //     TexPaint.tex2 = TexPaint.SchneckeTexTemp;
    //
    //     RenderSettings.skybox = TexPaint.SkyBoxSchnecke;
    //     DynamicGI.UpdateEnvironment();
    //     Probe.RenderProbe();
    //
    //     //statue.GetComponent<MeshRenderer>().material = baseMaterial;
    //     TexPaint.statue.GetComponent<MeshRenderer>().material.mainTexture = TexPaint.canvasTexture;
    //     //rendTex.GetComponent<MeshRenderer>().material = baseMaterial;
    //     TexPaint.rendTex.GetComponent<MeshRenderer>().material.mainTexture = TexPaint.tex;
    // }
    //
    // public void SockelJuengling(bool Active)
    // {
    //     DestroyStatueReal(TagPaint);
    //     DestroyStatueReal(TagInter);
    //
    //     //DestroyStatue(TagLoewe);
    //    // DestroyStatue(TagSchnecke);
    //    // DestroyStatue(TagJueng);
    //     loeweActive = false;
    //     schneckeActive = false;
    //     juenglingActive = Active;
    //     TexPaint.restoreMaterial();
    //     TexPaint.LöweStatue.SetActive(false);
    //     TexPaint.SchneckeStatue.SetActive(false);
    //     TexPaint.JuenglingStatue.SetActive(Active);
    //
    //     TexPaint.statue = TexPaint.JuenglingStatue;
    //     TexPaint.tex = TexPaint.JuenglingTex;
    //     TexPaint.tex2 = TexPaint.JuenglingTexTemp;
    //
    //     RenderSettings.skybox = TexPaint.SkyBoxJüngling;
    //     DynamicGI.UpdateEnvironment();
    //     Probe.RenderProbe();
    //
    //     //statue.GetComponent<MeshRenderer>().material = baseMaterial;
    //     TexPaint.statue.GetComponent<MeshRenderer>().material.mainTexture = TexPaint.canvasTexture;
    //     //rendTex.GetComponent<MeshRenderer>().material = baseMaterial;
    //     TexPaint.rendTex.GetComponent<MeshRenderer>().material.mainTexture = TexPaint.tex;
    // }


    public bool RequestStatueReset(StatueValues statue)
    {
        var wasCurrentStatue = false;
        if (statue == _currentStatue)
        {
            ResetStatuesAndSockets();
            _currentStatue = null;
            wasCurrentStatue = true;

            DestroyStatueReal(TagPaint);
            DestroyStatueReal(TagInter);
        }
        else
        {
            foreach (var socket in _sockets)
            {
                if (socket.StatueValues == statue)
                    socket.Reset();
            }

            wasCurrentStatue = false;
        }

        return wasCurrentStatue;
    }

    public void ResetStatuesAndSockets()
    {
        foreach (var socket in _sockets)
        {
            socket.Reset();
        }

        DestroyStatueReal(TagPaint);
        DestroyStatueReal(TagInter);

        //PassthroughManager.Reset();

        //soundManager.StopSound();

        foreach (var statue in _statueInstances)
        {
            var statueData = AvailableStatues.FirstOrDefault(values => values == statue.StatueValues);

            if (statueData != null)
            {
                statue.transform.localPosition = statueData.StatueSpawnPoint;
                statue.transform.localRotation = Quaternion.Euler(statueData.StatueSpawnRotation);
                statue.gameObject.SetActive(false);

                sliceController.removeOriginalStatue(statue);
                Debug.Log("Reset statue: " + statue.StatueValues.Title);
            }
            else
            {
                Debug.LogError("Data Table could not be found for statue: " + statue.StatueValues.Title);
            }

        }

        //TexPaint.ResetAll();

        _currentStatue = null;
    }

    public string GetCurrentName()
    {
        return _currentStatue ? _currentStatue.Title : "None";
    }

    public void SetSockelActive(StatueValues statue)
    {
        DestroyStatueReal(TagPaint);
        DestroyStatueReal(TagInter);
        AddOnController.ResetAddOns();
        //PassthroughManager.NewStatue(statue.Skybox);

        //soundManager.PlayStatueSound(_currentStatue.SoundClip,_currentStatue.ExplanationClip); 

        //TexPaint.restoreMaterial();

        var statueObject = _statueInstances.FirstOrDefault(stat => stat.StatueValues == _currentStatue);

        foreach (var stat in _statueInstances)
        {
            stat.gameObject.SetActive(stat.StatueValues == _currentStatue);

        }
        sliceController.addOriginalStatue(statueObject);

        foreach (var socket in _sockets)
        {
            if (socket.StatueValues != statue)
                socket.Reset();
        }


        //TexPaint.statue = statueObject.gameObject;
        var tex = (statue.Tex);
        var tempTex = (statue.TempTex);
        //TexPaint.tex = tex;
        //TexPaint.tex2 = tempTex;

        RenderSettings.skybox = statue.Skybox;

        DynamicGI.UpdateEnvironment();
        Probe.RenderProbe();

        //TexPaint.statue.GetComponent<MeshRenderer>().material.mainTexture = tex;
        //TexPaint.rendTex.GetComponent<MeshRenderer>().material.mainTexture = tempTex;
    }

}



