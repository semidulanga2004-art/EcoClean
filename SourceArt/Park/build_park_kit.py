import bpy, math, random, os
from mathutils import Vector
random.seed(31)
OUT=r'N:\unity\projects\EcoClean'
bpy.ops.object.select_all(action='DESELECT')
# Work in a separate scene, preserving the currently open scene.
scene=bpy.data.scenes.new('EcoClean Park Kit')
bpy.context.window.scene=scene
materials={}
for name,c in {'wood':(.32,.16,.065),'woodlight':(.56,.31,.12),'iron':(.12,.17,.16),'stone':(.64,.66,.59),'water':(.15,.58,.72),'leaf':(.28,.48,.09),'leaflight':(.43,.61,.13),'leafdark':(.14,.32,.09),'trunk':(.27,.14,.06),'blue':(.055,.28,.7),'yellow':(.95,.62,.055),'grey':(.26,.32,.34),'white':(.92,.91,.77),'red':(.75,.26,.12),'bottle':(.20,.65,.77)}.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);materials[name]=m
parts=[]
def mat(o,m):
 o.data.materials.append(materials[m]);parts.append(o);return o
def cube(name,p,s,m):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=s
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return mat(o,m)
def cyl(name,p,r,d,m,n=12,r2=None):
 bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r,radius2=r if r2 is None else r2,depth=d,location=p);o=bpy.context.object;o.name=name;return mat(o,m)
def ico(name,p,s,m):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=p);o=bpy.context.object;o.name=name;o.scale=s
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return mat(o,m)
def ring(name,r,w,z,depth,m,n=48):
 verts=[];faces=[]
 for zz in [z-depth/2,z+depth/2]:
  for rr in [r-w/2,r+w/2]:
   verts += [(rr*math.cos(i*2*math.pi/n),rr*math.sin(i*2*math.pi/n),zz) for i in range(n)]
 for i in range(n):
  j=(i+1)%n
  faces += [(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),(i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);return mat(o,m)
def finish(name):
 global parts
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Assets','EcoCleanPark','Models',name+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False)
 o.hide_set(True);parts=[]
 return o
# Original fountain, real bowls with raised rims, central pedestal.
cyl('foot',(0,0,.16),2.95,.32,'stone',48)
ring('basin wall',2.65,.30,.48,.55,'stone')
cyl('waterbed',(0,0,.34),2.5,.08,'water',48)
cyl('pedestal base',(0,0,.7),.60,.7,'stone',16,r2=.34)
cyl('pedestal',(0,0,1.35),.26,.85,'stone',12)
cyl('lower bowl',(0,0,1.7),.35,.3,'stone',24,r2=1.18)
ring('lower lip',1.12,.12,1.87,.12,'stone',32)
cyl('upper stem',(0,0,2.13),.17,.65,'stone',12)
cyl('upper bowl',(0,0,2.5),.18,.24,'stone',20,r2=.58)
ring('upper lip',.54,.09,2.64,.1,'stone',24)
cyl('nozzle',(0,0,2.78),.08,.25,'stone',10)
finish('Fountain')
for typ in ['Bench','PicnicTable']:
 if typ=='Bench':
  for x in [-.85,.85]:
   for y in [-.25,.25]:cube('leg',(x,y,.29),(.12,.12,.58),'iron')
  for y in [-.25,0,.25]:cube('seat',(0,y,.60),(2.25,.20,.10),'woodlight')
  for z in [.9,1.12]:cube('back',(0,.34,z),(2.25,.10,.17),'woodlight')
  for x in [-.85,.85]:cube('back support',(x,.37,.85),(.09,.09,.7),'iron')
 else:
  for x in [-.8,.8]:
   for y in [-.4,.4]:cube('leg',(x,y,.42),(.14,.14,.84),'wood')
   cube('crossbar',(x,0,.38),(.15,1.85,.12),'wood')
  for y in [-.4,-.2,0,.2,.4]:cube('table plank',(0,y,.87),(2.4,.18,.12),'woodlight')
  for y in [-.82,.82]:
   cube('seat',(0,y,.46),(2.4,.34,.10),'woodlight')
 finish(typ)
for name,m in [('BinPaper','blue'),('BinPlastic','yellow'),('BinMetal','grey')]:
 cube('body',(0,0,.65),(.85,.8,1.3),m);cube('lid',(0,0,1.35),(.96,.91,.12),m)
 cube('opening',(0,-.405,1.13),(.46,.018,.15),'iron')
 cube('label',(0,-.412,.7),(.52,.018,.40),'white')
 finish(name)
for name,m in [('TreeOak','leaf'),('TreeLime','leaflight'),('TreeDark','leafdark')]:
 cyl('trunk',(0,0,1.5),.21,3,'trunk',7,r2=.12)
 for p,s in [((0,0,3.7),(1.6,1.55,1.9)),((.85,.1,3.1),(1.2,1.25,1.35)),((-.7,.2,3.3),(1.1,1.1,1.5))]:ico('crown',p,s,m)
 finish(name)
for name,m in [('Bush','leaf'),('BushLight','leaflight')]:
 for x in [-.38,.3]:ico('leaves',(x,0,.5),(.65,.63,.65),m)
 finish(name)
ico('rock',(0,0,.26),(.65,.45,.45),'stone');finish('Rock')
for x in [-1.0,1.0]:cube('post',(x,0,.7),(.17,.17,1.4),'wood')
for z in [.5,1.0]:cube('rail',(0,0,z),(2,.12,.13),'woodlight')
finish('Fence')
cube('post',(0,0,.85),(.12,.12,1.7),'wood');cube('board',(0,0,1.55),(1.5,.14,.75),'woodlight');finish('Sign')
cyl('bottle',(0,0,.16),.09,.32,'bottle',8);cyl('neck',(0,0,.36),.04,.12,'bottle',8);cyl('cap',(0,0,.43),.045,.035,'blue',8);finish('LitterBottle')
cyl('cup',(0,0,.14),.075,.28,'white',10,r2=.11);cyl('lid',(0,0,.29),.12,.025,'wood',12);finish('LitterCup')
cube('paper',(0,0,.025),(.38,.26,.04),'white');finish('LitterPaper')
for a in range(5):
 angle=a*math.tau/5;ico('petal',(.12*math.cos(angle),.12*math.sin(angle),.36),(.12,.08,.06),'white')
cyl('stem',(0,0,.16),.018,.32,'leafdark',5);ico('centre',(0,0,.37),(.07,.07,.05),'yellow');finish('Flower')
for x in [-1.5,1.5]:cube('bed edge',(x,0,.15),(.15,2,.3),'woodlight')
for y in [-1,1]:cube('bed edge',(0,y,.15),(3,.15,.3),'woodlight')
cube('soil',(0,0,.08),(2.9,1.9,.15),'wood');finish('PlantingBed')
# Make the complete source library visible in a spaced grid for inspection.
for i,o in enumerate([o for o in scene.objects if o.type=='MESH']):
 o.hide_set(False);o.location=(i%5*8,i//5*8,0)
scene.world=bpy.data.worlds.new('Park daylight');scene.world.color=(.65,.72,.8)
for area in bpy.context.screen.areas:
 if area.type=='VIEW_3D':
  area.spaces.active.shading.color_type='MATERIAL';area.spaces.active.region_3d.view_distance=45;area.spaces.active.region_3d.view_location=(16,12,0)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'SourceArt','Park','EcoClean_Park_Kit.blend'))
open(os.path.join(OUT,'SourceArt','Park','build-complete.txt'),'w').write('Original kit exported successfully: '+str(len(scene.objects))+' assets')
