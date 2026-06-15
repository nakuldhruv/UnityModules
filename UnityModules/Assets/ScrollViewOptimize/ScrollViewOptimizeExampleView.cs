using System.Collections.Generic;
using UnityEngine;

public class ScrollViewOptimizeExampleView : MonoBehaviour, ScrollViewOptimize.IBehaviourHandler
{
    [SerializeField] private ScrollViewOptimize         m_ScrollViewOptimize;
    [SerializeField] private ScrollViewOptimizeItemView m_InstancePrefab;
    [SerializeField] private RectTransform              m_DummyPrefab;

    private ObjectPool<ScrollViewOptimizeItemView> m_ObjectPool;
    private ScrollViewOptimizeData                 m_Data;

    private void Awake()
    {
        m_Data       = new ScrollViewOptimizeData();
        m_ObjectPool = new ObjectPool<ScrollViewOptimizeItemView>(m_InstancePrefab);
        m_ScrollViewOptimize.Initialize(this, m_DummyPrefab);
        m_ScrollViewOptimize.OnUpdate();
    }

    public void OnUpdate(int idx, RectTransform rect)
    {
        rect.GetComponent<ScrollViewOptimizeItemView>().Refresh(m_Data.DataList[idx]);
    }

    public RectTransform Alloc(int index)
    {
        return m_ObjectPool.Alloc().GetComponent<RectTransform>();
    }

    public void Free(RectTransform rect)
    {
        m_ObjectPool.Release(rect.GetComponent<ScrollViewOptimizeItemView>());
    }

    public int Count => m_Data.DataList.Count;
}

public class ScrollViewOptimizeData
{
    public List<string> DataList = new List<string>()
    {
        "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Item8", "Item9", "Item10",
        "Item11", "Item12", "Item13", "Item14", "Item15", "Item16", "Item17", "Item18", "Item19", "Item20",
        "Item21", "Item22", "Item23", "Item24", "Item25", "Item26",
    };
}