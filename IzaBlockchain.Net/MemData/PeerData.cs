using IzaBlockchain.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IzaBlockchain.MemDatas;

public class PeerData : MemData
{
    public bool AddPeer(Peer peer)
    {
        var col = db.GetCollection<Peer>();

        if (col.Exists(p => p.IsEqual(peer)))
            return false;

        col.Insert(peer);

        return true;
    }
    public bool AddPeer(IPAddress ip)
    {
        var peer = Peer.From(ip);

        var col = db.GetCollection<Peer>();

        if (col.Exists(p => p.IsEqual(peer)))
            return false;

        col.Insert(peer);

        return true;
    }
    public bool RemovePeer(IPAddress ip)
    {
        var peer = Peer.From(ip);

        var col = db.GetCollection<Peer>();

        if (!col.Exists(p => p.IsEqual(peer)))
            return false;

        col.DeleteMany(p => p.IsEqual(peer));

        return true;
    }
    public bool RemovePeer(Peer peer)
    {
        var col = db.GetCollection<Peer>();

        if(!col.Exists(p => p.IsEqual(peer)))
            return false;

        col.DeleteMany(p => p.IsEqual(peer));

        return true;
    }

    public IEnumerable<Peer> GetAllPeers()
    {
        var col = db.GetCollection<Peer>();

        return col.FindAll();
    }
}