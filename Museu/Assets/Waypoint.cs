﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class Waypoint : MonoBehaviour
{

	private DateTime t_inicio;
	private TimeSpan t_result;
	private bool objetoEmLazer = false;
	private bool andando = false;
	public float speed = 0.3f;

	private enum State
	{
		Idle,
		Focused,
		Clicked,
		Approaching,
		Moving,
		Collect,
		Collected,
		Occupied,
		Open,
		Hidden
	}

	[SerializeField]
	private State  		_state					= State.Idle;
	private Color		_color_origional		= new Color(0.0f, 1.0f, 0.0f, 0.5f);
	private Color		_color					= Color.white;
	private float 		_scale					= 1.0f;
	private float 		_animated_lerp			= 1.0f;
	private AudioSource _audio_source			= null;
	private Material	_material				= null;

	[Header("Material")]
	public Material	material					= null;
	public Color color_hilight					= new Color(0.8f, 0.8f, 1.0f, 0.125f);	
	
	[Header("State Blend Speeds")]
	public float lerp_idle 						= 0.0f;
	public float lerp_focus 					= 0.0f;
	public float lerp_hide						= 0.0f;
	public float lerp_clicked					= 0.0f;
	
	[Header("State Animation Scales")]
	public float scale_clicked_max				= 0.0f;
	public float scale_animation				= 3.0f;	
	public float scale_idle_min 				= 0.0f;
	public float scale_idle_max 				= 0.0f;
	public float scale_focus_min				= 0.0f;
	public float scale_focus_max				= 0.0f;

	[Header("Sounds")]
	public AudioClip clip_click					= null;	
	public AudioClip narracao = null;

	[Header("Hide Distance")]
	public float threshold						= 0.125f;



	void Awake()
	{		
		_material					= Instantiate(material);
		_color_origional			= _material.color;
		_color						= _color_origional;
		_audio_source				= gameObject.GetComponent<AudioSource>();	
		_audio_source.clip		 	= clip_click;
		_audio_source.playOnAwake 	= false;
		_audio_source.loop = false;
	}


	void Update()
	{
		bool occupied 	= Camera.main.transform.parent.transform.position == gameObject.transform.position;



		switch(_state)
		{
			case State.Idle:
				Idle ();
				_audio_source.clip = clip_click;
				_state 		= occupied ? State.Occupied : _state;
				break;

			case State.Focused:
				Focus();
				break;

			case State.Clicked:
				Clicked();

				bool scaled = _scale >= scale_clicked_max * .95f;
				_state 		= scaled ? State.Approaching : _state;
				break;

			case State.Approaching:
				Hide();	

				_state 		= occupied ? State.Occupied : _state;
				break;
			case State.Occupied:
				Hide ();
					
				if (narracao != null) {

					Narrar ();

				}

				_state = !occupied ? State.Idle : _state;
				break;
			
			case State.Hidden:
				Hide();
				break;

			default:
				break;
		}

		Andar ();

		gameObject.GetComponentInChildren<MeshRenderer>().material.color 	= _color;
		gameObject.transform.localScale 									= Vector3.one * _scale;

		_animated_lerp														= Mathf.Abs(Mathf.Cos(Time.time * scale_animation));
	}

	private bool narrou = false;

	public void Narrar() {
		if (!_audio_source.isPlaying) {
			_audio_source.Play ();
			_audio_source.clip = narracao;
			narrou = true;
		}
	}


	public void Enter()
	{
		_state = _state == State.Idle ? State.Focused : _state;
		Debug.Log ("Dentro");
	}


	public void Exit()
	{
		_state = State.Idle;
		Debug.Log ("Fora");
		if (!andando) {
			Limpar ();
		}
	}


	public void Click()
	{
		_state = _state == State.Focused ? State.Clicked : _state;
		
		_audio_source.Play();

		if(!andando)
			andando = true;
		//Camera.main.transform.parent.transform.position = Vector3.Lerp(Camera.main.transform.parent.transform.position, gameObject.transform.position, speed);

		//Camera.main.transform.parent.transform.position = gameObject.transform.position;
	}


	private void Idle()
	{
		float scale				= Mathf.Lerp(scale_idle_min, scale_idle_max, _animated_lerp);
		Color color				= Color.Lerp(_color_origional, 	  color_hilight, _animated_lerp);

		_scale					= Mathf.Lerp(_scale, scale, lerp_idle);
		_color					= Color.Lerp(_color, color, lerp_idle);
	}


	public void Focus()
	{
		float scale				= Mathf.Lerp(scale_focus_min, scale_focus_max, _animated_lerp);
		Color color				= Color.Lerp(   _color_origional,   color_hilight, _animated_lerp);

		_scale					= Mathf.Lerp(_scale, scale, lerp_focus);
		_color					= Color.Lerp(_color, color,	lerp_focus);
		TriploView ();
	}


	public void Clicked()
	{	
		_scale					= Mathf.Lerp(_scale, scale_clicked_max, lerp_clicked);
		_color					= Color.Lerp(_color,     color_hilight, lerp_clicked);
	}


	public void Hide()
	{
		_scale					= Mathf.Lerp(_scale, 		0.0f, lerp_hide);
		_color					= Color.Lerp(_color, Color.clear, lerp_hide);
	}


	//minhas modificacoes

	private void Andar() {
		if(andando) {
			float distance = Vector3.Distance(Camera.main.transform.parent.transform.position, gameObject.transform.position);
			speed = Mathf.Clamp(speed, 0.01f, 0.9f);
			if (distance > 0.05f) {
				Camera.main.transform.parent.transform.position = Vector3.Lerp (Camera.main.transform.parent.transform.position, gameObject.transform.position, speed);
			} else {
				Camera.main.transform.parent.transform.position = gameObject.transform.position;
				Limpar ();
			}
		}
	}

	private void TriploView() {
		// conteudo
		if (!objetoEmLazer) {
			objetoEmLazer = true;
			t_inicio = DateTime.Now;
		} else {
			t_result = DateTime.Now.Subtract(t_inicio);
			int seegundos = (int) t_result.TotalSeconds;
			if (seegundos < 3) {
				Debug.Log (t_result.TotalSeconds.ToString ());
			} else {
				objetoEmLazer = false;
				Debug.Log ("Pular");
				Click();
				return;
			}
		}

	}

	private void Limpar() {
		andando = false;
		objetoEmLazer = false;
	}

	public void Imprimi(string s){
		Debug.Log (s);
	}
}
