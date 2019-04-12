using UnityEngine;
using System.Collections;
using TMPro;


public class ConsoleText : MonoBehaviour
{
    public enum Timing { Start, OnEnable }

    public bool useHierachy;

    public Timing timing = Timing.Start;
    public float startDelay = 1;

    [Range(0, 500)]
    public int revealSpeed = 50;

    private TMP_Text m_textMeshPro;
    int counter = 0;
    int visibleCount = 0;

    void Awake()
    {
        // Get Reference to TextMeshPro Component
        m_textMeshPro = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (timing == Timing.Start)
            StartCoroutine(Type());
    }

    private void OnEnable()
    {
        if (timing == Timing.OnEnable)
            StartCoroutine(Type());
    }

    IEnumerator Type()
    {
        m_textMeshPro.maxVisibleCharacters = 0;

        // Force and update of the mesh to get valid information.
        m_textMeshPro.ForceMeshUpdate();

        int totalVisibleCharacters = m_textMeshPro.textInfo.characterCount; // Get # of Visible Character in text object

        yield return new WaitForSeconds(useHierachy ? transform.GetSiblingIndex() : startDelay);

        while (true)
        {
            visibleCount = counter % (totalVisibleCharacters + 1);

            m_textMeshPro.maxVisibleCharacters = visibleCount; // How many characters should TextMeshPro display?
            if (counter < totalVisibleCharacters)
                counter++;
            else
                totalVisibleCharacters = m_textMeshPro.textInfo.characterCount;

            if (counter > totalVisibleCharacters)
                counter = totalVisibleCharacters;

            yield return new WaitForSeconds(1f / revealSpeed);
        }
    }
}
