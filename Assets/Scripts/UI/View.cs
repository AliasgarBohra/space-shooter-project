using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

public class View : MonoBehaviour
{
    [SerializeField] private Animations animationType;
    [SerializeField] private Ease openEase, closeEase;

    [Header("In how much time animation will complete")]
    [SerializeField] private float completionTime = 0.5f;

    [Header("Applicable to specific animations only")]
    [SerializeField] private bool oppositeAnimation = false;

    [Header("Must have CG on 1st child back and window as 2ndchild")]
    [SerializeField] private bool doBackFade = false;

    [Header("Animation play on 1st Child")]
    [SerializeField] private bool doOnChild = true;

    [HideInInspector] public Action onOpen, onClose;

    public UnityEvent onOpenAction;

    public UnityEvent whileOpenAction;
    public UnityEvent whileCloseAction;
    public UnityEvent onHideAction;

    [Header("Only check in Menu IMP panels")]
    [SerializeField] private bool playOnStart = false;

    public bool pushToStack = false;

    public bool closeViewOnBackPress = true;

    [SerializeField] private CanvasGroup otherCanvasGroup;

    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;

    private enum Animations
    {
        NONE,
        POPUP,
        SLIDE_HORIZONTAL,
        SLIDE_VERTICAL,
        FADE_ONLY
    }
    private void Start()
    {
        if (playOnStart)
        {
            gameObject.SetActive(false);
            Show();
        }
    }
    public void Show(int duration)
    {
        Show();

        Invoke(nameof(Hide), duration);
    }
    public void Show()
    {
        if (isAnimating || gameObject.activeSelf)
            return;

        isAnimating = true;
        whileOpenAction?.Invoke();

        switch (animationType)
        {
            case Animations.NONE:
                ForceShow();
                break;

            case Animations.POPUP:
                OpenPopup();
                break;

            case Animations.SLIDE_HORIZONTAL:
                SlideHorizontallyOpen();
                break;

            case Animations.SLIDE_VERTICAL:
                SlideVerticalOpen();
                break;

            case Animations.FADE_ONLY:
                OpenFade();
                break;
        }
    }
    public void Hide()
    {
        if (isAnimating || !gameObject.activeSelf)
            return;

        isAnimating = true;
        whileCloseAction?.Invoke();

        switch (animationType)
        {
            case Animations.NONE:
                ForceHide();
                break;

            case Animations.POPUP:
                ClosePopup();
                break;

            case Animations.SLIDE_HORIZONTAL:
                SlideHorizontallyClose();
                break;

            case Animations.SLIDE_VERTICAL:
                SlideVerticalClose();
                break;

            case Animations.FADE_ONLY:
                CloseFade();
                break;
        }
    }
    public void ShowAndHideCurrentView(View viewToShow)
    {
        onClose = delegate
        {
            viewToShow.Show();
        };
        Hide();
    }
    private void ShowGameObject()
    {
        gameObject.SetActive(true);
    }
    public void ForceShow()
    {
        ShowGameObject();
        isAnimating = false;
    }
    public void ForceHide()
    {
        ForceHideWithoutPopping();
    }
    public void ForceHideWithoutPopping()
    {
        isAnimating = false;

        if (!gameObject.activeSelf)
            return;

        gameObject.SetActive(false);

        onHideAction?.Invoke();
    }
    private void OnCloseComplete()
    {
        ForceHide();
        onClose?.Invoke();
    }
    private void OnOpenComplete()
    {
        isAnimating = false;
        onOpen?.Invoke();
        onOpenAction?.Invoke();
    }

    #region ANIMATIONS
    #region Popup Animation
    private void OpenPopup()
    {
        ShowGameObject();

        Transform panel = transform;

        if (doOnChild)
        {
            panel = transform.GetChild(0);
        }
        panel.localScale = Vector3.zero;
        panel.DOScale(1, completionTime).SetEase(openEase).OnComplete(OnOpenComplete);
    }
    private void ClosePopup()
    {
        Transform panel = transform;

        if (doOnChild)
        {
            panel = transform.GetChild(0);
        }
        panel.DOScale(0, completionTime).SetEase(closeEase).OnComplete(OnCloseComplete);
    }
    #endregion

    #region Slide Vertical Animation + Fade effect
    private void SlideVerticalOpen()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float sh = oppositeAnimation ? rect.rect.height : -rect.rect.height;

        if (doBackFade)
        {
            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<CanvasGroup>().DOFade(1, completionTime);
        }
        if (doOnChild)
        {
            rect = transform.GetChild(0).GetComponent<RectTransform>();
        }
        rect.anchoredPosition = new Vector2(0, sh);

        ShowGameObject();
        rect.DOAnchorPosY(0, completionTime).SetEase(openEase).OnComplete(OnOpenComplete);
    }

    private void SlideVerticalClose()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float sh = oppositeAnimation ? rect.rect.height : -rect.rect.height;

        if (doBackFade)
        {
            GetComponent<CanvasGroup>().DOFade(0, completionTime);
        }
        if (doOnChild)
        {
            rect = transform.GetChild(0).GetComponent<RectTransform>();
        }
        rect.DOAnchorPosY(sh, completionTime).SetEase(closeEase).OnComplete(OnCloseComplete);
    }

    #endregion

    #region Slide Horizontal Animation + Fade effect
    private void SlideHorizontallyOpen()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float sw = oppositeAnimation ? rect.rect.width : -rect.rect.width;

        if (doBackFade)
        {
            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<CanvasGroup>().DOFade(1, completionTime);
        }
        if (doOnChild)
        {
            rect = transform.GetChild(0).GetComponent<RectTransform>();
        }

        rect.anchoredPosition = new Vector2(sw, 0);

        ShowGameObject();
        rect.DOAnchorPosX(0, completionTime).SetEase(openEase).OnComplete(OnOpenComplete);
    }
    private void SlideHorizontallyClose()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float sw = oppositeAnimation ? rect.rect.width : -rect.rect.width;

        if (doBackFade)
        {
            GetComponent<CanvasGroup>().DOFade(0, completionTime);
        }
        if (doOnChild)
        {
            rect = transform.GetChild(0).GetComponent<RectTransform>();
        }
        rect.DOAnchorPosX(sw, completionTime).SetEase(closeEase).OnComplete(OnCloseComplete);
    }
    #endregion

    #region Fade Animation
    private void OpenFade()
    {
        ForceShow();

        GetComponent<CanvasGroup>().alpha = 0;
        GetComponent<CanvasGroup>().DOFade(1, completionTime).OnComplete(OnOpenComplete);

        if (otherCanvasGroup != null)
        {
            otherCanvasGroup.alpha = 0;
            otherCanvasGroup.DOFade(1, completionTime).OnComplete(OnOpenComplete);
        }
    }
    private void CloseFade()
    {
        GetComponent<CanvasGroup>().DOFade(0, completionTime).OnComplete(OnCloseComplete);

        if (otherCanvasGroup != null)
        {
            otherCanvasGroup.DOFade(0, completionTime).OnComplete(OnCloseComplete);
        }
    }
    #endregion
    #endregion
}