import sys, json, subprocess, math, wave
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).parent
sys.path.insert(0,str(root/'dependencies'))
import imageio_ffmpeg
ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
scenes=json.loads((root/'scenes.json').read_text())
font=lambda n:ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',n)
posters=[]
for i,s in enumerate(scenes):
 im=Image.new('RGB',(1920,1080),'#0a1425');d=ImageDraw.Draw(im)
 d.text((120,24),s[2],font=font(19),fill='#60dcc7')
 d.text((120,63),s[1],font=font(49),fill='white')
 d.text((1690,56),f'{i+1:02} / 08',font=font(22),fill='#9aadc2')
 screen=Image.open(root/f'screen-{i}.png').resize((1680,863),Image.Resampling.LANCZOS)
 im.paste(screen,(120,145));im.save(root/f'poster-{i}.png');posters.append(im)
rate=44100;t=np.arange(rate*60)/rate;audio=np.zeros(len(t));chords=[[130.81,164.81,196],[110,130.81,164.81],[87.31,110,130.81],[98,123.47,146.83]]
for beat in range(120):
 a=beat*rate//2;n=int(.48*rate);tt=np.arange(n)/rate;f=chords[(beat//8)%4][beat%3]*2
 env=np.minimum(tt/.025,1)*np.exp(-tt*9);audio[a:a+n]+=.12*np.sin(2*np.pi*f*tt)*env
audio*=np.minimum(t,1)*np.minimum(60-t,1)
with wave.open(str(root/'soundtrack.wav'),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes((audio*32767).astype('<i2').tobytes())
cmd=[ffmpeg,'-y','-f','rawvideo','-vcodec','rawvideo','-pix_fmt','rgb24','-s','1920x1080','-r','24','-i','-','-i',str(root/'soundtrack.wav'),'-c:v','libx264','-preset','veryfast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-t','60','-movflags','+faststart',str(root/'TMS-showcase.mp4')]
with open(root/'encode.log','w') as log:
 p=subprocess.Popen(cmd,stdin=subprocess.PIPE,stderr=log)
 elapsed=0
 for idx,(_,_,_,duration) in enumerate(scenes):
  for frame in range(duration*24):
   local=frame/24;absolute=elapsed+local
   im=posters[idx].copy();d=ImageDraw.Draw(im);d.rectangle((120,1040,1800,1043),fill='#31435a');d.rectangle((120,1040,120+1680*absolute/60,1043),fill='#60dcc7')
   fade=min(1,local/.35,(duration-local)/.35)
   if fade<1:im=Image.blend(Image.new('RGB',im.size,'#0a1425'),im,max(0,fade))
   p.stdin.write(im.tobytes())
  elapsed+=duration;print('Encoded scene',idx+1,flush=True)
 p.stdin.close();assert p.wait()==0
print(root/'TMS-showcase.mp4')
