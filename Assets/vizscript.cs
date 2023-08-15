using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class vizscript : MonoBehaviour
{
    public string s1;
    public float temp_sum, media_volume, media_volume_max, small_wave, big_wave, wave_diff, prev_wave_diff,fft_sum, big_wave_count,prev_big_wave_count,temp_count ;
    public float bass_timer, old_fft_sum, wave_di, wave_di_count, wavelength , prev_wavelength, big_wave_count_avg , sum4, hill_mul, wave_slope_count;
    public float[] spectrum, freq_avg, old_freq_avg, floatbyteswave, floatbyteswave4;
    public float[,] wave2darray;
    public int[] freqs,enemylr;
    public GameObject cube, textobject, testcube, enemy, bullet, bullet_prefab, gun, vizmesh, camera, sun, sphere;
    public GameObject[] freq_cubes,wavecubes, freq_spheres;
    public int specsize, bass_count, temp_sum_count, enemy_pos,vert,tris,wall_count,pit_count, update_counter;
    public int[] intbytes,intbyteswave;
    public Text t1;
    public bool left, right, left1, right1, isbass;
    public AudioSource[] asources;
    public AudioSource a1, a2, a3, a4, a5;
    public Color[] meshcolor;
    public Gradient meshgrad;
    public GradientColorKey[] meshgrad_colorkey;
    public GradientAlphaKey[] meshgrad_alphakey;

    static int STREAMMUSIC;
    static int FLAGSHOWUI = 1;


    public Mesh mymesh;
    public Vector3[] vertices,flatvertices;
    public int[] triangles;
    public int xsize,zsize;

    public ParticleSystem ps1;

    public Material sphere_mat;


    private static AndroidJavaObject audioManager;
    private AndroidJavaClass vizplugin;

    void Start()
    {

        xsize = 250;
        zsize = 100;
        mymesh = new Mesh();
        vizmesh.GetComponent<MeshFilter>().mesh = mymesh;
        vertices = new Vector3[(xsize + 1) * (zsize + 1)];
        wave2darray = new float[zsize + 1, xsize + 1];
        triangles = new int[xsize * zsize * 6];
        flatvertices = new Vector3[triangles.Length];

        createvizmesh();
        updatevizmesh();


        meshgrad = new Gradient();
        meshgrad_colorkey = new GradientColorKey[5];
        meshgrad_alphakey = new GradientAlphaKey[5];

        for (int i = 0; i < meshgrad_colorkey.Length; i++)
        {
            meshgrad_colorkey[i].color = new Color(UnityEngine.Random.Range(0f, 0.4f), UnityEngine.Random.Range(0f, 0.4f), UnityEngine.Random.Range(0f, 0.4f));
            meshgrad_colorkey[i].time = 0.1f * i;
            meshgrad_alphakey[i].alpha = 1;
            meshgrad_alphakey[i].time = 0.1f * i;
            meshgrad.SetKeys(meshgrad_colorkey,meshgrad_alphakey);
        }


        freq_spheres = new GameObject[300];
        enemylr = new int[] {1,-1};
        specsize = 512;
        freqs = new int[] { 1,2,3,4,5,6,7,8,16,24,32,48,64,96,128,160,192,224,256,320,384,448,512 };
        freq_avg = new float[freqs.Length];
        freq_cubes = new GameObject[freqs.Length];
        old_freq_avg = new float[freqs.Length];
        floatbyteswave = new float[1024];
        floatbyteswave4 = new float[300];


        asources = GetComponents<AudioSource>();
        a1 = asources[0];

        wavecubes = new GameObject[1024];



        //for (int i = 0; i < freqs.Length; i++)
        //{
        //    freq_cubes[i] = Instantiate(cube, new Vector3(i - (freqs.Length / 2), 0, 100), Quaternion.identity);
        //}

        //for (int i = 0; i < 1024; i++)
        //{
        //    wavecubes[i] = Instantiate(cube, new Vector3(i - 512, 0, 500), Quaternion.identity);
        //}

        //for (int i = 0; i < specsize; i++)
        //{
        //    freq_cubes[i] = Instantiate(cube, new Vector3(i - (specsize / 4), 0, 150), Quaternion.identity);
        //}


        if (Microphone.devices.Length <= 0)
        {
            Debug.LogWarning("Microphone not connected!");
        }


        vizplugin = new AndroidJavaClass("com.example.vizplugin2.vizpluginclass");
        s1 = vizplugin.CallStatic<string>("startviz", 10);

        t1 = textobject.GetComponent<Text>();



        //media_volume_max = GetDeviceMaxVolume();
        spawnsphere();
    }


    public void spawnsphere()
    {
        for (int i = 0; i < 180; i++)
        {
            freq_spheres[i] = Instantiate(sphere, new Vector3(10 * Mathf.Cos((float)(2*i) * (3.14f / 180)), 10 * Mathf.Sin((float)(2*i) * (3.14f / 180)) + 10, 155), Quaternion.identity);
        }
    }

    public void freqsphere()
    {
        for (int i = 0; i < 180; i++)
        {
            freq_spheres[i].transform.position = new Vector3(floatbyteswave4[i] * wavelength / 1400 * Mathf.Cos((float)(2 * i) * (3.14f / 180)), floatbyteswave4[i] * wavelength / 1400 * Mathf.Sin((float)(2 * i) * (3.14f / 180)) + 10, 155);
        }
    }


    public void createvizmesh()
    {
        

        for (int i = 0, z = 0; z <= zsize; z++)
        {
            for (int x = 0; x <= xsize; x++)
            {
                vertices[i] = new Vector3(x, wave2darray[z, x], z);
                //vertices[i] = new Vector3(x, 0, z);
                i++;
            }
        }


        vert = 0;
        tris = 0;

        for (int z = 0; z < zsize; z++)
        {
            for (int x = 0; x < xsize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xsize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xsize + 1;
                triangles[tris + 5] = vert + xsize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        meshcolor = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= zsize; z++)
        {
            for (int x = 0; x <= xsize; x++)
            {
                meshcolor[i] = meshgrad.Evaluate((vertices[i].y / 40));// * (big_wave_count/400));
                i++;
            }
        }


        //for (int i = 0; i < triangles.Length; i++)
        //{
        //    flatvertices[i] = vertices[triangles[i]];
        //    triangles[i] = i;
        //}


    }

    public void updatevizmesh()
    {
        
        mymesh.Clear();
        mymesh.vertices = vertices;
        mymesh.triangles = triangles;
        mymesh.colors = meshcolor;

        mymesh.RecalculateNormals();
       
        
    }





    void Update()
    {
        DetectMeshCollsion();


        //t1.text = floatbyteswave4[249].ToString();
        //GetComponent<Light>().intensity = big_wave_count / 600 ;


        if (sun.GetComponent<Light>().intensity > 1)
        {
            sun.GetComponent<Light>().intensity -= 0.03f;
        }


        //movement();
        movement1();

        media_volume = GetDeviceVolume();
        media_volume /= media_volume_max;
        media_volume *= 100;


        camera.transform.position = new Vector3(transform.position.x,camera.transform.position.y,camera.transform.position.z);


        //intbytes = vizplugin.CallStatic<int[]>("getviz", 10);
        intbyteswave = vizplugin.CallStatic<int[]>("getvizwave", 10);

        //for (int i = 0; i < 1024; i++)
        //{
        //    wavecubes[i].transform.position = new Vector3(wavecubes[i].transform.position.x, intbyteswave[i], wavecubes[i].transform.position.z);
        //}


        big_wave_count = 0;
        temp_count = 0;
        sum4 = 0;
        for (int i = 1, j = 0; i < 1024; i++)
        {
            floatbyteswave[i] = Mathf.Abs(intbyteswave[i]);
            sum4 += floatbyteswave[i];

            if (i % 4 == 0)
            {
                sum4 /= 4;
                j = i / 4;
                floatbyteswave4[j] = sum4;
                sum4 = 0;
            }


            if (floatbyteswave[i] > 115)
            {
                if (temp_count > big_wave_count)
                {
                    big_wave_count = temp_count;
                }
                temp_count = 0;
            }
            else
                temp_count++;

        }
        prev_wavelength = wavelength;
        wavelength = big_wave_count;
        testcube.transform.localScale = new Vector3(2,big_wave_count/10,2);


        if (((wavelength > 2 * prev_wavelength) && (wavelength > 70 + prev_wavelength) && (wavelength > 200)) ||
           ((wavelength > 1.3f * prev_wavelength) && (wavelength > 50 + prev_wavelength) && (wavelength > 300)))
        {
            isbass = true;
        }

        ps1.startSpeed = 40 + (wavelength / 1);
        ps1.emissionRate = 20 + (wavelength / 2);

        bassfn();


        big_wave_count_avg = (big_wave_count_avg + big_wave_count) / 2;


        for (int i = 2; i < xsize - 1; i++)
        {
            floatbyteswave4[i] = (floatbyteswave4[i] + floatbyteswave4[i - 1]) / 2;
        }

        //specsize = intbytes.Length / 2;

        //float[] fft = new float[specsize];
        //for (int i = 0; i < specsize; i++)
        //{
        //    float real = intbytes[(i * 2) + 0];
        //    float imag = intbytes[(i * 2) + 1];
        //    fft[i] = (Mathf.Sqrt((real * real) + (imag * imag)));
        //}
        //spectrum = fft;




        //temp_sum = 0;
        //for (int i = 0; i < 512; i++)
        //{
        //    temp_sum += spectrum[i];
        //}
        //old_fft_sum = fft_sum;
        //fft_sum = temp_sum;
        //fft_sum /= 512;
        //t1.text = fft_sum.ToString();
        //testcube.transform.localScale = new Vector3(3,fft_sum * 100,3);



        //for (int i = 0; i < freqs.Length - 1; i++)
        //{
        //    temp_sum = 0;
        //    for (int j = freqs[i]; j < freqs[i + 1]; j++)
        //    {
        //        temp_sum += spectrum[j];
        //    }
        //    temp_sum /= (freqs[i + 1] - freqs[i]);
        //    old_freq_avg[i] = freq_avg[i];
        //    freq_avg[i] = temp_sum;

        //    //if (i < 7)
        //    //    freq_cubes[i].transform.localScale = new Vector3(1, freq_avg[i] / 2, 1);
        //    //else
        //    //    freq_cubes[i].transform.localScale = new Vector3(1, (freq_avg[i] * i * i) / 30, 1);
        //}





        //bass_count = 0;
        //for (int i = 0; i < 8; i++)
        //{
        //    if (((freq_avg[i] > old_freq_avg[i] * 7) && (freq_avg[i] > old_freq_avg[i] + 60)) || ((freq_avg[i] > old_freq_avg[i] * 5) && (freq_avg[i] > old_freq_avg[i] + 100)))
        //    {
        //        bass_count++;
        //        //t1.text = (freq_avg[i] - old_freq_avg[i]).ToString();
        //    }
        //}



        //if (bass_count >= 1)
        //{
        //    isbass = true;
        //}



        //bassfn();














        for (int i = 0; i < zsize; i++)
        {
            for (int j = 0; j < xsize; j++)
            {
                wave2darray[i, j] = wave2darray[i + 1, j];
            }
        }


        //float tempval = 0;
        //for (int j = 0; j < xsize; j++)
        //{
        //    if ((Mathf.Abs(intbyteswave[4 * j]) > 100) && (big_wave_count > 250))
        //        tempval = (big_wave_count / 1600) * (Mathf.Abs(intbyteswave[4 * j]));
        //    else
        //        tempval = (big_wave_count / 1700) * (Mathf.Abs(intbyteswave[4 * j]));

        //    if (wave2darray[zsize, j] > tempval)
        //    {
        //        wave2darray[zsize, j] = ((wave2darray[zsize, j] / 1.1f));// + (Mathf.Abs(intbyteswave[4 * j]) / 40));
        //    }
        //    else
        //    {
        //        wave2darray[zsize, j] = tempval;
        //        wave2darray[zsize - 1, j] = (tempval + wave2darray[zsize - 1, j]) / 2;
        //        wave2darray[zsize - 2, j] = (wave2darray[zsize - 1, j] + wave2darray[zsize - 2, j]) / 2;
        //        wave2darray[zsize - 3, j] = (wave2darray[zsize - 2, j] + wave2darray[zsize - 3, j]) / 2;
        //        wave2darray[zsize - 4, j] = (wave2darray[zsize - 3, j] + wave2darray[zsize - 4, j]) / 2;
        //        wave2darray[zsize - 5, j] = (wave2darray[zsize - 4, j] + wave2darray[zsize - 5, j]) / 2;
        //    }
        //}

        float tempval = 0;
        float elevation_value = 0;
        float tempval_max = 0;
        float tempval_sum = 0;
        for (int j = 1; j < xsize; j++)
        {
            if ((floatbyteswave4[j] > 100) && (big_wave_count > 250))
            {
                tempval = (big_wave_count / 1400) * (floatbyteswave4[j]) * hill_mul;
            }
            else
            {
                tempval = (big_wave_count / 1700) * (floatbyteswave4[j]) * hill_mul;
            }

            tempval_sum += tempval;

            if (tempval > tempval_max)
            {
                tempval_max = tempval;
            }

            if ((tempval_max > 50)&&(hill_mul == 2))
            {
                hill_mul = 1;
            }

            if ((tempval_sum / j > 20) && (j > xsize / 3))
            {
                hill_mul = 0.5f;
            }

            if (((tempval_sum / j < 20) && (j > 2 * (xsize / 3))) && (hill_mul == 0.5f))
            {
                hill_mul = 1;
            }

            elevation_value = Mathf.Abs(j - (xsize / 2));

            if (elevation_value > xsize / 4)
            {
                tempval += (Mathf.Pow((elevation_value - xsize / 4), 2) / 100);
            }




            if (wave2darray[zsize, j] > tempval)
            {
                wave_slope_count++;

                wave2darray[zsize, j] = (wave2darray[zsize, j] - tempval) / (1 + (wave_slope_count/100)) + tempval;

            }
            else
            {
                wave_slope_count = 0;

                wave2darray[zsize, j] = tempval;

                for (int k = 1; k < 10; k++)
                {
                    if (wave2darray[zsize - k, j] < wave2darray[zsize - k + 1, j])
                        wave2darray[zsize - k, j] = wave2darray[zsize - k, j] + (wave2darray[zsize - k + 1, j] - wave2darray[zsize - k, j]) / (1 + ((float)k / 10));

                }
                //wave2darray[zsize - 1, j] = (tempval + wave2darray[zsize - 1, j]) / 2;
                //wave2darray[zsize - 2, j] = (wave2darray[zsize - 1, j] + wave2darray[zsize - 2, j]) / 2;
                //wave2darray[zsize - 3, j] = (wave2darray[zsize - 2, j] + wave2darray[zsize - 3, j]) / 2;
                //wave2darray[zsize - 4, j] = (wave2darray[zsize - 3, j] + wave2darray[zsize - 4, j]) / 2;
                //wave2darray[zsize - 5, j] = (wave2darray[zsize - 4, j] + wave2darray[zsize - 5, j]) / 2;
            }
        }

        if (tempval_max < 30)
        {
            hill_mul = 2;
        }
        else
        {
            hill_mul = 1;
        }





        //wall_count = 0;
        //pit_count = 0;
        //for (int j = 0; j < xsize; j++)
        //{

        //    if (pit_count > 0)
        //    {
        //        wave2darray[zsize, j] = (int)((big_wave_count / 9000) * (Mathf.Abs(intbyteswave[3 * j])));
        //        pit_count--;
        //    }
        //    else
        //    {
        //        if (Mathf.Abs(intbyteswave[3 * j]) > 80)
        //        {
        //            wall_count++;
        //            if (wall_count > 20)
        //            {
        //                pit_count = 40;
        //                wall_count = 0;
        //            }
        //            else
        //            {
        //                int h = (int)((big_wave_count / 1000) * (Mathf.Abs(intbyteswave[3 * j])));

        //                if (h > 40)
        //                    h = 40;

        //                wave2darray[zsize, j] = h;

        //            }
        //        }
        //        else
        //            wave2darray[zsize, j] = (int)((big_wave_count / 9000) * (Mathf.Abs(intbyteswave[3 * j])));
        //    }


        //}









        createvizmesh();
        updatevizmesh();
        freqsphere();
        //t1.text = media_volume.ToString();
    }








    public void bassfn()
    {
        if (isbass)
        {

            if (Time.time > bass_timer + 0.3f)
            {
                for (int i = 0; i < meshgrad_colorkey.Length; i++)
                {
                    if (i == 0)
                    {
                        meshgrad_colorkey[i].color = new Color(UnityEngine.Random.Range(0f, 0.6f), UnityEngine.Random.Range(0f, 0.6f), UnityEngine.Random.Range(0f, 0.6f));
                    }
                    else if (i == 1)
                    {
                        meshgrad_colorkey[i].color = new Color(UnityEngine.Random.Range(0f, 0.05f), UnityEngine.Random.Range(0f, 0.05f), UnityEngine.Random.Range(0f, 0.05f));
                    }
                    else if (i == 2)
                    {
                        meshgrad_colorkey[i].color = new Color(UnityEngine.Random.Range(0f, 0.2f), UnityEngine.Random.Range(0f, 0.2f), UnityEngine.Random.Range(0f, 0.2f));
                    }
                    else if (i == 3)
                    {
                        meshgrad_colorkey[i].color = new Color(UnityEngine.Random.Range(0f, 0.1f), UnityEngine.Random.Range(0f, 0.1f), UnityEngine.Random.Range(0f, 0.1f));
                    }
                    else
                    {
                        meshgrad_colorkey[i].color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    }


                    meshgrad_colorkey[i].time = 0.25f * i;
                    meshgrad_alphakey[i].alpha = 1;
                    meshgrad_alphakey[i].time = 0.25f * i;
                    meshgrad.SetKeys(meshgrad_colorkey, meshgrad_alphakey);

                    if (i == meshgrad_colorkey.Length-1)
                    {
                        ps1.startColor = meshgrad_colorkey[i].color;
                        sphere_mat.color = meshgrad_colorkey[i].color;
                    }
                }



                sun.GetComponent<Light>().intensity = 1.4f;
                //a1.PlayOneShot(a1.clip, 0.7f);
                bass_timer = Time.time;;
                bullet = Instantiate(bullet_prefab, new Vector3(0 , 10 , 150), Quaternion.identity);
                bullet.transform.LookAt(transform.position);
                bullet.GetComponent<Rigidbody>().AddForce((bullet.transform.position - transform.position) * -120);

                //changepos();
                //enemy.transform.position = new Vector3(enemy_pos, 1, 80);
                isbass = false;
            }
        }
    }


    public void changepos()
    {
        if (enemy_pos == 8)
            enemy_pos -= 4;
        else if (enemy_pos == -8)
            enemy_pos += 4;
        else
            enemy_pos += (4 * enemylr[Random.Range(0,2)]);
    }





    public void movement1()
    {
        float angle = transform.localEulerAngles.z;
        angle = (angle > 180) ? angle - 360 : angle;

        if (Input.touchCount != 0)
        {
            //transform.position = new Vector3(((Input.GetTouch(0).position.x / Screen.width) * 60) - 30, ((Input.GetTouch(0).position.y / Screen.height) * 30) - 15, transform.position.z);

            if (Input.GetTouch(0).position.x > (Screen.width / 2) + 40)
            {
                transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);

                if (angle > -30)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z - 4);
            }
            else if (Input.GetTouch(0).position.x < (Screen.width / 2) - 40)
            {
                transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);

                if (angle < 30)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + 4);
            }
            else
            {
                if (angle < -2)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + 4);
                else if (angle > 3)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z - 4);
            }
        }
        else
        {
            if (angle < -2)
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + 4);
            else if (angle > 3)
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z - 4);
        }


    }



    private void DetectMeshCollsion()
    {
        if (Mathf.Abs(transform.position.x) < 125)
            if (wave2darray[Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.x) + 125] > transform.position.y + 35)
                t1.text = wave2darray[Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.x) + 125].ToString();
    }








    private static AndroidJavaObject deviceAudio
    {
        get
        {
            if (audioManager == null)
            {
                AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaClass audioManagerClass = new AndroidJavaClass("android.media.AudioManager");
                AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");

                STREAMMUSIC = audioManagerClass.GetStatic<int>("STREAM_MUSIC");
                string Context_AUDIO_SERVICE = contextClass.GetStatic<string>("AUDIO_SERVICE");

                audioManager = context.Call<AndroidJavaObject>("getSystemService", Context_AUDIO_SERVICE);
            }
            return audioManager;
        }

    }

    private static int GetDeviceVolume()
    {
        return deviceAudio.Call<int>("getStreamVolume", STREAMMUSIC);
    }

    private static int GetDeviceMaxVolume()
    {
        return deviceAudio.Call<int>("getStreamMaxVolume", STREAMMUSIC);
    }



}
