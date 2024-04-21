##### Updating linux kernel
##### amneziawg installing
##### installing amneziawg-tools
##### setting AmneziaWG: awg0 + client config
_______________________________

##### patching linux kernel
#### tip: ubuntu server 22.04 is recommended

#0
apt update
apt dist-upgrade
reboot
//apt install --install-recommends linux-generic-hwe-22.04
apt install linux-headers-$(uname -r)

#1
apt autoremove -y
uname -r

#2
apt-get update
apt-get full-upgrade -y

#3 make sure at least one repo ("deb-src...") exists and is not commented
vi /etc/apt/sources.list

#4
apt install -y software-properties-common python3-launchpadlib gnupg2 linux-headers-$(uname -r)
apt install -y dkms





##### Installing amneziawg

# Put "amneziawg-X.tar" in the /root
# then:
tar xf amneziawg-X.tar
mv amneziawg-X /usr/src
dkms install amneziawg/X
modprobe amneziawg
lsmod | grep amneziawg





##### Installing amneziawg-tools
cd /root
git clone https://github.com/amnezia-vpn/amneziawg-tools
cd /root/amneziawg-tools/src
make /root/amneziawg-tools/src
make install

# check if we successfully installed amneziawg-tools
awg-quick -v




##### Setting AmneziaWG

#0 Delete default awg0
ip li add dev awg0 type amneziawg




# Use script or software to create awg0.conf (server) config (interface)
...



# Launch awg0
sysctl -w net.ipv4.conf.all.forwarding=1
//sysctl -w net.ipv6.conf.all.forwarding=1
sysctl -p

awg-quick down awg0
awg-quick up awg0
awg



# How to refresh awg0 without clients sessions interruptions:
awg syncconf awg0 <(awg-quick strip awg0)



_______________________________________________________
##### Problems?
##### Delete all AmneziaWG files (AWG remains in the kernel):
sudo apt purge amneziawg amneziawg-tools amneziawg-dkms