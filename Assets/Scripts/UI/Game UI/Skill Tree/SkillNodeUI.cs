using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image),typeof(Button))]
public class SkillNodeUI : MonoBehaviour
{
    private Button _button;
    private Image _skillImage;
    [SerializeField] private SkillNode _skillNode;
    public SkillNode SkillNode => _skillNode;
    // Start is called before the first frame update
    private void Start()
    {
        _button = GetComponent<Button>();
        _skillImage = GetComponent<Image>();
        _button.onClick.AddListener(Upgrade);
        if(_skillNode.IsLocked && _skillNode.Unlockable) _skillImage.color = new Color(_skillImage.color.r, _skillImage.color.g, _skillImage.color.b, 0.5f);
        else _skillImage.color = new Color(_skillImage.color.r, _skillImage.color.g, _skillImage.color.b, 0.1f);
    }

    private void Upgrade()
    {
        if (_skillNode.Unlockable) { 
            _skillImage.color = new Color(_skillImage.color.r, _skillImage.color.g, _skillImage.color.b, 1.0f);
            _skillNode.ApplyUpgrade();
            SkillTreeManager.OnSkillUpgrade.Invoke(this);
        }
    }
    public void UpdateUpgrade()
    {
        if (_skillNode.IsLocked && _skillNode.Unlockable) _skillImage.color = new Color(_skillImage.color.r, _skillImage.color.g, _skillImage.color.b, 0.5f);
        else _skillImage.color = new Color(_skillImage.color.r, _skillImage.color.g, _skillImage.color.b, 0.1f);
    }
}
