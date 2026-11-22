using UnityEngine;

// A shadow monster that floats after the player. If it touches you, you die and get reset to the stump.
public class Monster : MonoBehaviour
{
    public bool frozen;                 // "Freeze Monster" mod
    GtagBootstrap boot;
    float grace;
    Transform visual;
    const float KillDistance = 0.75f;

    public static Monster Create(GtagBootstrap b)
    {
        var go = new GameObject("Monster");
        go.layer = GtagBootstrap.PlayerLayer;
        var m = go.AddComponent<Monster>();
        m.boot = b;
        m.Build();
        return m;
    }

    void Build()
    {
        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);

        Part(PrimitiveType.Sphere, new Vector3(0f, 0f, 0f), new Vector3(1.1f, 1.4f, 1.1f), new Color(0.03f, 0.02f, 0.04f));
        Part(PrimitiveType.Sphere, new Vector3(-0.2f, 0.35f, 0.42f), Vector3.one * 0.18f, new Color(1f, 0.05f, 0.05f));
        Part(PrimitiveType.Sphere, new Vector3(0.2f, 0.35f, 0.42f), Vector3.one * 0.18f, new Color(1f, 0.05f, 0.05f));
        Part(PrimitiveType.Capsule, new Vector3(-0.75f, -0.1f, 0.2f), new Vector3(0.25f, 0.8f, 0.25f), new Color(0.05f, 0.03f, 0.06f)).localRotation = Quaternion.Euler(30f, 0f, 25f);
        Part(PrimitiveType.Capsule, new Vector3(0.75f, -0.1f, 0.2f), new Vector3(0.25f, 0.8f, 0.25f), new Color(0.05f, 0.03f, 0.06f)).localRotation = Quaternion.Euler(30f, 0f, -25f);

        var lightGo = new GameObject("Glow");
        lightGo.transform.SetParent(transform, false);
        var l = lightGo.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.1f, 0.1f);
        l.range = 6f;
        l.intensity = 1.2f;
        l.shadows = LightShadows.None;
    }

    Transform Part(PrimitiveType type, Vector3 pos, Vector3 scale, Color c)
    {
        var p = GameObject.CreatePrimitive(type);
        Destroy(p.GetComponent<Collider>());
        p.layer = GtagBootstrap.PlayerLayer;
        p.transform.SetParent(visual, false);
        p.transform.localPosition = pos;
        p.transform.localScale = scale;
        p.GetComponent<Renderer>().material.color = c;
        return p.transform;
    }

    public void SetActive(bool on)
    {
        gameObject.SetActive(on);
        if (on) Respawn();
    }

    // far from the player and the stump, then a few seconds of mercy
    public void Respawn()
    {
        Vector3 spot = boot.SpawnPoint + Vector3.up * 2f;
        for (int i = 0; i < 25; i++)
        {
            Vector3 g = boot.FindGround(1);
            if ((g - boot.SpawnPoint).sqrMagnitude > 25f * 25f && (g - boot.Head.position).sqrMagnitude > 20f * 20f)
            {
                spot = g + Vector3.up * 1.2f;
                break;
            }
        }
        transform.position = spot;
        grace = 4f;
    }

    void Update()
    {
        if (boot == null || boot.Player == null || !boot.Player.enabled) return;
        if (grace > 0f) grace -= Time.deltaTime;

        Vector3 target = boot.Head.position + Vector3.down * 0.25f;
        Vector3 to = target - transform.position;
        float dist = to.magnitude;

        if (dist < KillDistance && grace <= 0f && !boot.Invincible && !boot.IsDying)
        {
            boot.Die();
            return;
        }

        if (!frozen && !boot.IsDying && dist > 0.01f)
        {
            float speed = PlayerProfile.MonsterSpeed * (grace > 0f ? 0.5f : 1f);
            transform.position += to / dist * Mathf.Min(speed * Time.deltaTime, dist);
        }

        // never sink into the ground
        RaycastHit hit;
        Vector3 p = transform.position;
        if (Physics.Raycast(p + Vector3.up * 6f, Vector3.down, out hit, 40f, 1 << 0, QueryTriggerInteraction.Ignore))
            if (p.y < hit.point.y + 0.9f) { p.y = hit.point.y + 0.9f; transform.position = p; }

        Vector3 flat = new Vector3(to.x, 0f, to.z);
        if (flat.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 6f * Time.deltaTime);
        visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.12f, 0f);
    }
}
