using UnityEngine;

public class PageChangeLevelSelector : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    private int currentPage = 0;

    void Start()
    {
        DisplayPage(0);
    }

    public void DisplayPage(int index)
    {
        currentPage = Mathf.Clamp(index, 0, pages.Length - 1);
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == currentPage);
    }

    public void NextPage()
    {
        DisplayPage(currentPage + 1);
    }

    public void PreviousPage()
    {
        DisplayPage(currentPage - 1);
    }
}