using UnityEngine;
namespace LitEngine
{
    public class PlayAudioManager : MonoManagerBase
    {
        private static PlayAudioManager sInstance = null;
        static private void CreatInstance()
        {
            if (sInstance == null)
            {
                UnityEngine.GameObject tobj = new UnityEngine.GameObject("PlayAudioManager");
                UnityEngine.GameObject.DontDestroyOnLoad(tobj);
                sInstance = tobj.AddComponent<PlayAudioManager>();
                AppCore.AddPublicMono("PlayAudioManager", sInstance);
            }
        }
        private float mMusicValue = 1;
        public float MusicValue {
            get { return mMusicValue; }
            set
            {
                mMusicValue = Mathf.Clamp01(value);
                mBackMusic.volume = mMusicValue;
            }
        }
        private float mSoundValue = 1;
        public float SoundValue
        {
            get { return mSoundValue; }
            set
            {
                mSoundValue = Mathf.Clamp01(value);
                for (int i = 0; i < mMaxSoundCount; i++)
                {
                    mAudioSounds[i].volume = mSoundValue;
                }
            }
        }

        private AudioSource mBackMusic;
        private AudioSource[] mAudioSounds;
        private int mMaxSoundCount = 3;
        private int mIndex = 0;

        private void Awake()
        {
            mBackMusic = gameObject.AddComponent<AudioSource>();
            mAudioSounds = new AudioSource[mMaxSoundCount];
            for(int i = 0;i< mMaxSoundCount; i++)
            {
                mAudioSounds[i] = gameObject.AddComponent<AudioSource>();
            }
        }
        override protected void OnDestroy()
        {
            sInstance = null;
            base.OnDestroy();
        }

        public void PlaySound(AudioClip _clip)
        {
            if (sInstance == null)
                CreatInstance();
            if (mIndex == mMaxSoundCount) mIndex = 0;
            mAudioSounds[mIndex].Stop();
            mAudioSounds[mIndex].clip = _clip;
            mAudioSounds[mIndex].loop = false;
            mAudioSounds[mIndex].Play();
            mIndex++;
        }

        public void LoopSound(AudioClip _clip)
        {
            if (sInstance == null)
                CreatInstance();
            mBackMusic.Stop();
            mBackMusic.clip = _clip;
            mBackMusic.loop = true;
            mBackMusic.Play();
        }

        public void StopLoopSound()
        {
            mBackMusic.Stop();
        }

        public void StopSound()
        {
            for (int i = 0; i < mMaxSoundCount; i++)
            {
                mAudioSounds[i].Stop();
            }
        }

        public void ClearAllSound()
        {
            mBackMusic.Stop();
            mBackMusic.clip = null;
            for (int i = 0; i < mMaxSoundCount; i++)
            {
                mAudioSounds[i].Stop();
                mAudioSounds[i].clip = null;
            }
        }

        #region static
        static public void SetLoopVolume(float _volume)
        {
            if (sInstance == null)
                CreatInstance();
            sInstance.MusicValue = _volume;
        }
        static public void SetSoundVolume(float _volume)
        {
            if (sInstance == null)
                CreatInstance();
            sInstance.SoundValue = _volume;
        }
        static public void Play(AudioClip _clip)
        {
            if (sInstance == null)
                CreatInstance();
            sInstance.PlaySound(_clip);
        }

        static public void PlayLoop(AudioClip _clip)
        {
            if (sInstance == null)
                CreatInstance();
            sInstance.LoopSound(_clip);
        }

        static public void StopLoop()
        {
            if (sInstance == null) return;
            sInstance.StopLoopSound();
        }

        static public void StopAll()
        {
            if (sInstance == null) return;
            sInstance.StopLoopSound();
            sInstance.StopSound();
        }

        static public void Clear()
        {
            if (sInstance == null) return;
        }
        #endregion


    }
}
