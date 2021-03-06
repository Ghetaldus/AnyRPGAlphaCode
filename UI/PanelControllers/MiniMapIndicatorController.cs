using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
public class MiniMapIndicatorController : MonoBehaviour {

    [SerializeField]
    private GameObject miniMapTextLayerPrefab;

    [SerializeField]
    private GameObject miniMapImageLayerPrefab;

    [SerializeField]
    private Transform contentParent;

    private Interactable interactable;

    private Canvas canvas;

    private RectTransform rectTransform;

    private Vector2 uiOffset;

    private Dictionary<IInteractable, GameObject> miniMapLayers = new Dictionary<IInteractable, GameObject>();

    private bool setupComplete = false;

    protected bool eventSubscriptionsInitialized = false;

    private void Awake() {
        //Debug.Log("MiniMapIndicatorController.Awake()");
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvas.worldCamera = CameraManager.MyInstance.MyMiniMapCamera;
        canvas.planeDistance = 1f;
        uiOffset = new Vector2((float)rectTransform.sizeDelta.x / 2f, (float)rectTransform.sizeDelta.y / 2f);
        //Debug.Log("MiniMapIndicatorController.Awake(): rectTransform.sizeDelta: " + rectTransform.sizeDelta + "; uiOffset" + uiOffset);
    }

    private void Start() {
        //Debug.Log("MiniMapIndicatorController.Start()");
        CreateEventSubscriptions();
    }

    private void CreateEventSubscriptions() {
        //Debug.Log("PlayerManager.CreateEventSubscriptions()");
        if (eventSubscriptionsInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
        //SystemEventManager.MyInstance.OnReputationChange += HandleReputationChange;
        eventSubscriptionsInitialized = true;
    }

    private void CleanupEventSubscriptions() {
        //Debug.Log("MiniMapIndicatorController.CleanupEventSubscriptions()");
        if (!eventSubscriptionsInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
        //SystemEventManager.MyInstance.OnReputationChange -= HandleReputationChange;
        foreach (IInteractable _interactable in interactable.MyInteractables) {
            if (_interactable.HasMiniMapIcon() || _interactable.HasMiniMapText()) {
                _interactable.MiniMapStatusUpdateHandler -= HandleMiniMapStatusUpdate;
            }
        }
        eventSubscriptionsInitialized = false;
    }

    public void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventSubscriptions();
    }

    public void SetupMiniMap() {
        //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.SetupMiniMap()");
        if (setupComplete == true) {
            return;
        }
        if (interactable == null) {
            //Debug.Log(".MiniMapIndicatorController.Start(): interactable is null");
            return;
        }
        foreach (IInteractable _interactable in interactable.MyInteractables) {
            // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
            if (_interactable.HasMiniMapIcon()) {
                //else if (_interactable.HasMiniMapIcon()) {
                // do both now!
                //Debug.Log(interactable.MyName + ".MiniMapIndicatorController.Start(): interactable has minimapicon");
                GameObject go = Instantiate(miniMapImageLayerPrefab, contentParent);
                Image _image = go.GetComponent<Image>();
                _interactable.SetMiniMapIcon(_image);
                miniMapLayers.Add(_interactable, go);
            } else if (_interactable.HasMiniMapText()) {
                //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.Start(): interactable has minimaptext");
                GameObject go = Instantiate(miniMapTextLayerPrefab, contentParent);
                Text _text = go.GetComponent<Text>();
                _interactable.SetMiniMapText(_text);
                miniMapLayers.Add(_interactable, go);
            }
            if (_interactable.HasMiniMapIcon() || _interactable.HasMiniMapText()) {
                //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.SetupMiniMap(): adding minimap status handler");
                _interactable.MiniMapStatusUpdateHandler += HandleMiniMapStatusUpdate;
            } else {
                //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.SetupMiniMap(): unit had no icon or text, not setting up status handler");
            }
        }
        setupComplete = true;
    }

    public void SetInteractable(Interactable interactable) {
        //Debug.Log("MiniMapIndicatorController.SetNamePlateUnit(" + characterUnit.MyDisplayName + ")");
        this.interactable = interactable;
        SetupMiniMap();
    }

    private void LateUpdate() {
        //Debug.Log("MiniMapIndicatorController.LateUpdate(): interactable: " + (interactable == null ? "null" : (interactable.MyName == string.Empty ? interactable.name : interactable.MyName)) );
        if (setupComplete == false) {
            //Debug.Log("MiniMapIndicatorController.LateUpdate(): namePlateUnit: " + (interactable == null ? "null" : interactable.MyName) + ": setup has not completed yet!");
            return;
        }
        Vector2 viewportPosition = CameraManager.MyInstance.MyMiniMapCamera.WorldToViewportPoint(interactable.gameObject.transform.position);
        Vector2 proportionalPosition = new Vector2(viewportPosition.x * rectTransform.sizeDelta.x, viewportPosition.y * rectTransform.sizeDelta.y);
        //Debug.Log(interactable.gameObject.name + ".MiniMapIndicatorController.LateUpdate(). interactable position: " + interactable.gameObject.transform.position + "; viewportPosition: " + viewportPosition + "; proportionalPosition: " + proportionalPosition);
        contentParent.localPosition = proportionalPosition - uiOffset;
    }

    public void HandleMiniMapStatusUpdate(IInteractable _interactable) {
        //Debug.Log(_interactable.MyName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate()");
        if (miniMapLayers[_interactable] == null) {
            //Debug.Log(_interactable.MyName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate(): miniMapLayers[_interactable] is null! Exiting");
            return;
        }
        // this only supports one or the other too - prioritizing images
        //if (_interactable.GetCurrentOptionCount() > 0) {
            if (_interactable.HasMiniMapIcon()) {
            //Debug.Log(_interactable.MyName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate() : hasicon");
            _interactable.SetMiniMapIcon(miniMapLayers[_interactable].GetComponent<Image>());
            } else if (_interactable.HasMiniMapText()) {
            //Debug.Log(_interactable.MyName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate() : hastext");
            _interactable.SetMiniMapText(miniMapLayers[_interactable].GetComponent<Text>());
            }
        //}
    }

    public void HandleLevelUnload() {
        //Debug.Log("MiniMapIndicatorController.HandleLevelUnload(): interactable: " + interactable.MyName);
        Destroy(gameObject, 0);
    }

    /*
    private void OnDestroy() {
        if (characterUnit != null) {
            characterUnit.MyMiniMapIndicator = null;
        }
    }
    */
    /*
    public void OnPointerEnter(BaseEventData eventData) {
        if (characterUnit.MyInteractable != null) {
            characterUnit.MyInteractable.OnMouseEnter();
        }
    }

    public void OnPointerExit(BaseEventData eventData) {
        if (characterUnit.MyInteractable != null) {
            characterUnit.MyInteractable.OnMouseExit();
        }
    }
    */

}

}